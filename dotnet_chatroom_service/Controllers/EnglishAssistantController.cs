using Common.Dto;
using Common.Params.EnglishAssistant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace dotnet_chatroom_service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EnglishAssistantController : ControllerBase
{
    private readonly IEnglishAssistantService _englishAssistantService;

    public EnglishAssistantController(IEnglishAssistantService englishAssistantService)
    {
        _englishAssistantService = englishAssistantService;
    }

    [HttpPost("WordTips")]
    [Authorize]
    public Task<IActionResult> WordTips([FromBody] WordAssistanParams fetchAiArticleParams)
    {
        return Task.FromResult<IActionResult>(new StreamedResult(async (outputStream, cancellationToken) =>
        {
            try
            {
                await _englishAssistantService.WordTips(outputStream, fetchAiArticleParams, cancellationToken);
            }
            finally
            {
                await outputStream.DisposeAsync();
            }
        }, "text/event-stream"));
    }

    [HttpPost("WordTranslate")]
    [Authorize]
    public Task<IActionResult> WordTranslate([FromBody] WordAssistanParams fetchAiArticleParams)
    {
        return Task.FromResult<IActionResult>(new StreamedResult(async (outputStream, cancellationToken) =>
        {
            try
            {
                await _englishAssistantService.WordTranslate(outputStream, fetchAiArticleParams, cancellationToken);
            }
            finally
            {
                await outputStream.DisposeAsync();
            }
        }, "text/event-stream"));
    }

    [HttpPost("WordAssistan")]
    [Authorize]
    public Task<IActionResult> WordAssistan([FromBody] WordAssistanParams fetchAiArticleParams)
    {
        return Task.FromResult<IActionResult>(new StreamedResult(async (outputStream, cancellationToken) =>
        {
            try
            {
                await _englishAssistantService.WordAssistan(outputStream, fetchAiArticleParams, cancellationToken);
            }
            finally
            {
                await outputStream.DisposeAsync();
            }
        }, "text/event-stream"));
    }

    [HttpPost("TextLinguisticAssistant")]
    [Authorize]
    public Task<IActionResult> TextLinguisticAssistant([FromBody] TextLinguisticAssistantParams param)
    {
        return Task.FromResult<IActionResult>(new StreamedResult(async (outputStream, cancellationToken) =>
        {
            try
            {
                await _englishAssistantService.TextLinguisticAssistant(outputStream, param, cancellationToken);
            }
            finally
            {
                await outputStream.DisposeAsync();
            }
        }, "text/event-stream"));
    }
}
