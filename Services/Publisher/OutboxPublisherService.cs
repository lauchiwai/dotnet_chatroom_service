using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Repositories.MyDbContext;
using System.Text;

namespace Services.Publish;
public class OutboxPublisherService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxPublisherService> _logger;

    public OutboxPublisherService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxPublisherService> logger,
        IConfiguration configuration)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    private (string Exchange, string RoutingKey, string QueueName, string DlExchange, string DlRoutingKey, string DlQueueName) GetEventConfig(string eventType)
    {
        switch (eventType)
        {
            case "ChatSessionDeleted":
                return (
                    Exchange: "chat_events",
                    RoutingKey: "chat.deleted",
                    QueueName: "chat_deleted_queue",
                    DlExchange: "chat_dlx",
                    DlRoutingKey: "chat.dead",
                    DlQueueName: "chat_dlx_queue"
                );
            case "ArticleDeleted":
                return (
                    Exchange: "article_events",
                    RoutingKey: "article.deleted",
                    QueueName: "article_deleted_queue",
                    DlExchange: "article_dlx",
                    DlRoutingKey: "article.dead",
                    DlQueueName: "article_dlx_queue"
                );
            default:
                throw new NotSupportedException($"Unsupported event type: {eventType}");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("發件箱發佈服務正在啟動");
        var publishTask = PublishMessages(stoppingToken);
        var consumeDeadLettersTask = ConsumeDeadLetters(stoppingToken);
        await Task.WhenAll(publishTask, consumeDeadLettersTask);
        _logger.LogInformation("發件箱發佈服務正在停止");
    }

    private async Task PublishMessages(CancellationToken stoppingToken)
    {
        _logger.LogInformation("訊息發佈任務已開始");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("正在檢查未發佈的訊息");

            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                var messages = await context.OutboxMessage
                    .Where(m => !m.IsPublished && m.RetryCount < 3)
                    .OrderBy(m => m.CreatedTime)
                    .Take(100)
                    .ToListAsync(stoppingToken);

                _logger.LogInformation("找到 {Count} 條訊息需要處理", messages.Count);

                foreach (var message in messages)
                {
                    _logger.LogInformation("正在處理訊息 {MessageId}，類型為 {EventType}", message.Id, message.EventType);

                    try
                    {
                        var eventConfig = GetEventConfig(message.EventType);
                        _logger.LogInformation("已檢索到 {EventType} 的事件配置", message.EventType);

                        var factory = new ConnectionFactory
                        {
                            HostName = _configuration["RabbitmqConfig:HostName"],
                            UserName = _configuration["RabbitmqConfig:UserName"],
                            Password = _configuration["RabbitmqConfig:Password"]
                        };
                        using var connection = factory.CreateConnection();
                        using var channel = connection.CreateModel();

                        _logger.LogInformation("已連接到 RabbitMQ");

                        // 啟用 Publisher Confirms
                        channel.ConfirmSelect();

                        // 宣告持久化 Queue
                        channel.QueueDeclare(
                            queue: eventConfig.QueueName,
                            durable: true,
                            exclusive: false,
                            autoDelete: false,
                            arguments: new Dictionary<string, object> {
                                { "x-dead-letter-exchange", eventConfig.DlExchange },
                                { "x-dead-letter-routing-key", eventConfig.DlRoutingKey }
                            }
                        );

                        // 死信交換器宣告
                        channel.ExchangeDeclare(
                            exchange: eventConfig.DlExchange,
                            type: ExchangeType.Direct,
                            durable: true,
                            autoDelete: false
                        );

                        // 綁定 Queue
                        channel.QueueBind(
                            queue: eventConfig.QueueName,
                            exchange: eventConfig.Exchange,
                            routingKey: eventConfig.RoutingKey
                        );

                        // 設定訊息持久化
                        var properties = channel.CreateBasicProperties();
                        properties.Persistent = true;

                        _logger.LogInformation("正在將訊息發佈到交換機 {Exchange}，路由鍵為 {RoutingKey}",
                            eventConfig.Exchange, eventConfig.RoutingKey);

                        // 發佈訊息
                        channel.BasicPublish(
                            exchange: eventConfig.Exchange,
                            routingKey: eventConfig.RoutingKey,
                            mandatory: true,
                            basicProperties: properties,
                            body: Encoding.UTF8.GetBytes(message.Payload)
                        );

                        // 等待確認
                        if (channel.WaitForConfirms(TimeSpan.FromSeconds(5)))
                        {
                            _logger.LogInformation("訊息 {MessageId} 已被代理確認", message.Id);
                            message.IsPublished = true;
                        }
                        else
                        {
                            _logger.LogError("訊息 {MessageId} 未被代理確認", message.Id);
                            throw new Exception("代理確認失敗");
                        }
                        await context.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("訊息 {MessageId} 已標記為成功發佈", message.Id);
                    }
                    catch (Exception ex)
                    {
                        message.RetryCount++;
                        _logger.LogError(ex, "發佈訊息 {MessageId} 失敗，重試次數: {RetryCount}", message.Id, message.RetryCount);
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
            }
            _logger.LogInformation("等待5秒後進行下一次檢查");
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ConsumeDeadLetters(CancellationToken stoppingToken)
    {
        _logger.LogInformation("死信消費者任務已開始");

        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitmqConfig:HostName"],
            UserName = _configuration["RabbitmqConfig:UserName"],
            Password = _configuration["RabbitmqConfig:Password"]
        };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        _logger.LogInformation("已連接到 RabbitMQ 以消費死信");

        var eventTypes = new[] { "ChatSessionDeleted", "ArticleDeleted" };
        foreach (var eventType in eventTypes)
        {
            var config = GetEventConfig(eventType);
            _logger.LogInformation("正在為 {EventType} 設置死信消費者", eventType);

            channel.QueueDeclare(
                queue: config.DlQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            channel.QueueBind(
                queue: config.DlQueueName,
                exchange: config.DlExchange,
                routingKey: config.DlRoutingKey
            );

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                _logger.LogInformation("收到 {EventType} 的死信訊息", eventType);

                try
                {
                    var body = ea.Body.ToArray();
                    var payload = Encoding.UTF8.GetString(body);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "處理死信訊息失敗");
                    channel.BasicNack(ea.DeliveryTag, false, false); // 處理失敗，不重新排隊，避免循環
                    _logger.LogInformation("因錯誤拒絕了死信訊息");
                }
            };

            channel.BasicConsume(
                queue: config.DlQueueName,
                autoAck: false, // 手動確認訊息
                consumer: consumer
            );

            _logger.LogInformation("已開始消費 {EventType} 的死信", eventType);
        }

        _logger.LogInformation("所有死信消費者已啟動，等待訊息中...");

        // 保持消費者運行，直到服務停止
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        _logger.LogInformation("死信消費者任務正在停止");
    }
}
