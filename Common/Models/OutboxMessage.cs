namespace Common.Models;

public partial class OutboxMessage
{
    /// <summary>
    /// 訊息編號
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// 事件
    /// </summary>
    public string EventType { get; set; } = null!;

    /// <summary>
    /// 資料
    /// </summary>
    public string Payload { get; set; } = null!;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// : 0:true, 1: false
    /// 是否已經推送過了
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// 重新送出次數
    /// </summary>
    public int RetryCount { get; set; }
}
