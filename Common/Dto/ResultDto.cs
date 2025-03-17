namespace Common.Dto;
public class ResultDTO
{
    /// <summary>
    /// 表示操作是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 操作結果的訊息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 操作結果的資料
    /// </summary>
    public object Data { get; set; }

    /// <summary>
    /// 錯誤訊息
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// 預設建構函式
    /// </summary>
    public ResultDTO( ) {}

    /// <summary>
    /// 帶有初始化參數的建構函式
    /// </summary>
    /// <param name="isSuccess">是否成功</param>
    /// <param name="message">訊息</param>
    /// <param name="data">資料</param>
    /// <param name="errorMessage">錯誤訊息</param>
    public ResultDTO(bool isSuccess, string message, object data = null, string errorMessage = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Data = data;
        ErrorMessage = errorMessage;
    }
}
