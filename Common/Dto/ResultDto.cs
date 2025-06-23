namespace Common.Dto;

/// <summary>
/// 通用操作结果DTO（泛型版本）
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class ResultDTO<T>
{
    /// <summary>
    /// 表示操作是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 狀態碼
    /// </summary>
    public int Code { get; set; } = 200;

    /// <summary>
    /// 操作結果的訊息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 操作結果的資料（泛型）
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// 預設建構函式
    /// </summary>
    public ResultDTO() { }

    /// <summary>
    /// 帶有初始化參數的建構函式
    /// </summary>
    /// <param name="isSuccess">是否成功</param>
    /// <param name="code">狀態碼</param>
    /// <param name="message">訊息</param>
    /// <param name="data">資料</param>
    public ResultDTO(bool isSuccess, int code = 200, string message = "", T data = default)
    {
        IsSuccess = isSuccess;
        Code = code;
        Message = message;
        Data = data;
    }

    /// <summary>
    /// 创建成功结果（简化方法）
    /// </summary>
    public static ResultDTO<T> Success(T data, string message = "操作成功")
        => new ResultDTO<T>(true, 200, message, data);

    /// <summary>
    /// 创建失败结果（简化方法）
    /// </summary>
    public static ResultDTO<T> Fail(string message, int code = 500, T data = default)
        => new ResultDTO<T>(false, code, message, data);
}

/// <summary>
/// 非泛型版本（兼容旧代码，相当于 ResultDTO<object>）
/// </summary>
public class ResultDTO : ResultDTO<object>
{
    public ResultDTO() { }

    public ResultDTO(bool isSuccess, int code = 200, string message = "", object data = null)
        : base(isSuccess, code, message, data) { }

    /// <summary>
    /// 创建成功结果（简化方法）
    /// </summary>
    public static ResultDTO Success(object data, string message = "操作成功")
        => new ResultDTO(true, 200, message, data);

    /// <summary>
    /// 创建失败结果（简化方法）
    /// </summary>
    public static ResultDTO Fail(string message, int code = 500, object data = null)
        => new ResultDTO(false, code, message, data);
}
