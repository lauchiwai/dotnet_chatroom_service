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
    public async Task<IActionResult> WordTips([FromBody] WordAssistanParams fetchAiArticleParams)
    {
        return new StreamedResult(async (outputStream, cancellationToken) =>
        {
            try
            {
                await _englishAssistantService.WordTips(outputStream, fetchAiArticleParams, cancellationToken);
            }
            finally
            {
                outputStream.Close();
            }
        }, "text/event-stream");
    }

    [HttpPost("WordTranslate")]
    [Authorize]
    public async Task<IActionResult> WordTranslate([FromBody] WordAssistanParams fetchAiArticleParams)
    {
        return new StreamedResult(async (outputStream, cancellationToken) =>
        {
            try
            {
                await _englishAssistantService.WordTranslate(outputStream, fetchAiArticleParams, cancellationToken);
            }
            finally
            {
                outputStream.Close();
            }
        }, "text/event-stream");
    }

    [HttpPost("WordAssistan")]
    [Authorize]
    public async Task<IActionResult> WordAssistan([FromBody] WordAssistanParams fetchAiArticleParams)
    {
        return new StreamedResult(async (outputStream, cancellationToken) =>
        {
            try
            {
                await _englishAssistantService.WordAssistan(outputStream, fetchAiArticleParams, cancellationToken);
            }
            finally
            {
                outputStream.Close();
            }
        }, "text/event-stream");
    }

    [HttpPost("TextLinguisticAssistant")]
    [Authorize]
    public async Task<IActionResult> TextLinguisticAssistant([FromBody] TextLinguisticAssistantParams param)
    {
        return new StreamedResult(async (outputStream, cancellationToken) =>
        {
            try
            {
                await _englishAssistantService.TextLinguisticAssistant(outputStream, param, cancellationToken);
            }
            finally
            {
                outputStream.Close();
            }
        }, "text/event-stream");
    }
}
