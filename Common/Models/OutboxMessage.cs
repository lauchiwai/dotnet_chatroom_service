namespace Common.Models;

public partial class OutboxMessage
{
    public string Id { get; set; } = null!;

    public string EventType { get; set; } = null!;

    public string Payload { get; set; } = null!;

    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// : 0:true, 1: false
    /// </summary>
    public bool IsPublished { get; set; }

    public int RetryCount { get; set; }
}
