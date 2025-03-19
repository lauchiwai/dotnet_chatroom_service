using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace dotnet_chatroom_service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatSessionController : ControllerBase
{
    private readonly IChatSessionService _chatSessionService;

    public ChatSessionController(IChatSessionService chatSessionService)
    {
        _chatSessionService = chatSessionService;
    }

    [HttpPost("GenerateChatSession")]
    [Authorize]
    public async Task<IActionResult> GenerateChatSession(string userTimeZoneId = "Asia/Hong_Kong")
    {
        var result = await _chatSessionService.GenerateChatSession(userTimeZoneId);

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }
}
