using Common.Params;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace dotnet_chatroom_service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticateController : ControllerBase
{
    private readonly IAuthenticateService _authenticateService;

    public AuthenticateController(IAuthenticateService authenticateService)
    {
        _authenticateService = authenticateService;
    }

    [HttpPost("RamdomRegister")]
    public async Task<IActionResult> RamdomRegister()
    {
        var result = await _authenticateService.RamdomRegister();

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterParams registerFrom)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authenticateService.Register(registerFrom);

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginParams loginFrom)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }


        var result = await _authenticateService.Login(loginFrom);

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpPost("Refresh")]
    public async Task<IActionResult> RefreshToken(string refreshToken)
    {
        var result = await _authenticateService.RefreshToken(refreshToken);
        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }
}
