// OutboxPublisherService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
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

    private (string Exchange, string RoutingKey, string QueueName, string DlExchange, string DlRoutingKey) GetEventConfig(string eventType)
    {
        switch (eventType)
        {
            case "ChatSessionDeleted":
                return (
                    Exchange: "chat_events",
                    RoutingKey: "chat.deleted",
                    QueueName: "chat_deleted_queue",
                    DlExchange: "chat_dlx",
                    DlRoutingKey: "chat.dead"
                );
            case "ArticleDeleted":
                return (
                    Exchange: "article_events",
                    RoutingKey: "article.deleted",
                    QueueName: "article_deleted_queue",
                    DlExchange: "article_dlx",
                    DlRoutingKey: "article.dead"
                );
            default:
                throw new NotSupportedException($"Unsupported event type: {eventType}");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();

                var messages = await context.OutboxMessage
                    .Where(m => !m.IsPublished && m.RetryCount < 3)
                    .OrderBy(m => m.CreatedTime)
                    .Take(100)
                    .ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    try
                    {

                        var eventConfig = GetEventConfig(message.EventType);

                        var factory = new ConnectionFactory
                        {
                            HostName = _configuration["RabbitmqConfig:HostName"],
                            UserName = _configuration["RabbitmqConfig:UserName"],
                            Password = _configuration["RabbitmqConfig:Password"]
                        };
                        using var connection = factory.CreateConnection();
                        using var channel = connection.CreateModel();

                        // 啓用 Publisher Confirms
                        channel.ConfirmSelect();

                        // 聲明持久化 Queue
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

                        // 死信交换器声明
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

                        // 設定消息持久化
                        var properties = channel.CreateBasicProperties();
                        properties.Persistent = true;

                        // 發佈消息
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
                            _logger.LogInformation("Message confirmed.");
                            message.IsPublished = true;
                        }
                        else
                        {
                            _logger.LogError("Message not confirmed.");
                            throw new Exception("Broker confirmation failed.");
                        }

                        await context.SaveChangesAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        message.RetryCount++;
                        _logger.LogError(ex, "Failed to publish message {MessageId}", message.Id);
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
            }
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}