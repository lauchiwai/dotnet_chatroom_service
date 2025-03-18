using Common.Dto;
using Common.Params;
using Microsoft.AspNetCore.Mvc;

namespace Services.Interfaces;

public interface IAuthenticateService
{
    /// <summary>
    /// 注冊
    /// </summary>
    /// <param name="registerFrom"></param>
    /// <returns></returns>
    Task<ResultDTO> Register([FromBody] RegisterParams registerFrom);
}
