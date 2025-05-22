namespace Common.ViewModels.Authenticate;

public class TokenViewModel
{
    /// <summary>
    /// jwt token
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// 刷新用 token
    /// </summary>
    public string RefreshToken { get; set; }
}
