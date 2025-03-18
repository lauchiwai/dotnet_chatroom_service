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
}
