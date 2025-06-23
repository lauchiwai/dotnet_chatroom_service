namespace Common.HttpClientResultDto;

public class ChatServiceHttpClientResultDto
{
    /// <summary>
    /// 表示操作是否成功
    /// </summary>
    public bool success { get; set; }

    /// <summary>
    /// 操作結果的訊息
    /// </summary>
    public string message { get; set; } = null!;

    /// <summary>
    /// 操作結果的資料
    /// </summary>
    public object data { get; set; } = null!;
}
