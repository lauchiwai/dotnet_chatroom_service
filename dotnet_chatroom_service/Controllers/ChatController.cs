using Common.Dto;
using Common.Params;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace dotnet_chatroom_service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost("GenerateChatSession")]
    [Authorize]
    public async Task<IActionResult> GenerateChatSession(string userTimeZoneId = "Asia/Hong_Kong")
    {
        var result = await _chatService.GenerateChatSession(userTimeZoneId);

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpGet("GetChatSessionList")]
    [Authorize]
    public async Task<IActionResult> GetChatSessionList()
    {
        var result = await _chatService.GetChatSessionList();

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpGet("CheackChatHttpClientHealth")]
    public async Task<IActionResult> CheackChatHttpClientHealth()
    {
        var result = await _chatService.CheackChatHttpClientHealth();

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpGet("GetChatHistoryBySessionId/{sessionId}")]
    [Authorize]
    public async Task<IActionResult> GetChatHistoryBySessionId(string sessionId)
    {
        var validateResult = await _chatService.ValidateChatPermission(sessionId);
        if (!validateResult.IsSuccess)
        {
            return BadRequest(validateResult);
        }

        var result = await _chatService.GetChatHistoryBySessionId(sessionId);
        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpDelete("DeleteChatData/{sessionId}")]
    [Authorize]
    public async Task<IActionResult> DeleteChatData(string sessionId)
    {
        var validateResult = await _chatService.ValidateChatPermission(sessionId);
        if (!validateResult.IsSuccess)
        {
            return BadRequest(validateResult);
        }

        var result = await _chatService.DeleteChatData(sessionId);
        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpPost("RefreshChatSessionTime")]
    [Authorize]
    public async Task<IActionResult> RefreshChatSessionTime(string sessionId)
    {
        var result = await _chatService.RefreshChatSessionTime(sessionId);

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpPost("ChatStream")]
    [Authorize]
    public async Task<IActionResult> ChatStream([FromBody] ChatParams chatParams)
    {
        return new StreamedResult(async (outputStream, cancellationToken) =>
        {
            try
            {
                await _chatService.ChatStream(outputStream, chatParams, cancellationToken);
            }
            finally
            {
                outputStream.Close();
            }
        }, "text/event-stream");
    }
}
