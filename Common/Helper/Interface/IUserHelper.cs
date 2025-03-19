namespace Common.Helper.Interface;

public interface IUserHelper
{
    public T ParseToken<T>() where T : new();
}
