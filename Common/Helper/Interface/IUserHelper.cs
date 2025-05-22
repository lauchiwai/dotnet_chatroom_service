namespace Common.Helper.Interface;

public interface IUserHelper
{
    /// <summary>
    /// 解析token 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T ParseToken<T>() where T : new();
}
