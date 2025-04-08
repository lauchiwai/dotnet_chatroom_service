using Common.Dto;
using Common.Params;
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

    [HttpGet("GetChatSessionList")]
    [Authorize]
    public async Task<IActionResult> GetChatSessionList()
    {
        var result = await _chatSessionService.GetChatSessionList();

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpGet("CheackChatHttpClientHealth")]
    public async Task<IActionResult> CheackChatHttpClientHealth()
    {
        var result = await _chatSessionService.CheackChatHttpClientHealth();

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpGet("GetChatHistoryBySessionId/{sessionId}")]
    [Authorize]
    public async Task<IActionResult> GetChatHistoryBySessionId(string sessionId)
    {
        var result = await _chatSessionService.GetChatHistoryBySessionId(sessionId);

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpDelete("DeleteChatData/{sessionId}")]
    [Authorize]
    public async Task<IActionResult> DeleteChatData(string sessionId)
    {
        var result = await _chatSessionService.DeleteChatData(sessionId);

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpPost("Chat")]
    [Authorize]
    public async Task<IActionResult> Chat([FromBody] ChatParams chatParams)
    {
        var result = await _chatSessionService.Chat(chatParams);

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }
}
