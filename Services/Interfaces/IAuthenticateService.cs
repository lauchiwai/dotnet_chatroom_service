﻿using Common.Dto;
using Common.Params.Authenticate;
using Microsoft.AspNetCore.Mvc;

namespace Services.Interfaces;

public interface IAuthenticateService
{
    /// <summary>
    /// 隨機生成一個賬號
    /// </summary>
    /// <returns></returns>
    public Task<ResultDTO> RamdomRegister();

    /// <summary>
    /// 注冊
    /// </summary>
    /// <param name="registerFrom"></param>
    /// <returns></returns>
    Task<ResultDTO> Register([FromBody] RegisterParams registerFrom);

    /// <summary>
    /// 登入
    /// </summary>
    /// <returns></returns>
    Task<ResultDTO> Login([FromBody] LoginParams loginFrom);

    /// <summary>
    /// 更新 Token
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    Task<ResultDTO> RefreshToken(string refreshToken);
}
