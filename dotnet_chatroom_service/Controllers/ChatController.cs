﻿using Common.Dto;
using Common.Params.Chat;
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
    public async Task<IActionResult> GenerateChatSession()
    {
        var result = await _chatService.GenerateChatSession();

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpPost("GenerateRagChatSession/{articleId}")]
    [Authorize]
    public async Task<IActionResult> GenerateRagChatSession(int articleId)
    {
        var result = await _chatService.GenerateRagChatSession(articleId);

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

    [HttpGet("GetRagChatSessionListByArticleId/{articleId}")]
    [Authorize]
    public async Task<IActionResult> GetRagChatSessionListByArticleId(int articleId)
    {
        var result = await _chatService.GetRagChatSessionListByArticleId(articleId);

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpGet("GetChatHistory/{sessionId}")]
    [Authorize]
    public async Task<IActionResult> GetChatHistory(int sessionId)
    {
        var validateResult = await _chatService.ValidateChatPermission(sessionId);
        if (!validateResult.IsSuccess)
        {
            return BadRequest(validateResult);
        }

        var result = await _chatService.GetChatHistory(sessionId);
        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpDelete("DeleteChatData/{sessionId}")]
    [Authorize]
    public async Task<IActionResult> DeleteChatData(int sessionId)
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
    public async Task<IActionResult> RefreshChatSessionTime(int sessionId)
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

    [HttpPost("SummaryStream")]
    [Authorize]
    public async Task<IActionResult> SummaryStream([FromBody] SummaryParams summaryParams)
    {
        return new StreamedResult(async (outputStream, cancellationToken) =>
        {
            try
            {
                await _chatService.SummaryStream(outputStream, summaryParams, cancellationToken);
            }
            finally
            {
                outputStream.Close();
            }
        }, "text/event-stream");
    }
}
