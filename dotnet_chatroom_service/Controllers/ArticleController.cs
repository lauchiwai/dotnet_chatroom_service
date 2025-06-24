using Common.Dto;
using Common.Params.Article;
using Common.Params.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Implementation;
using Services.Interfaces;

namespace dotnet_chatroom_service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ArticleController : ControllerBase
{
    private readonly IArticleService _articleService;

    public ArticleController(IArticleService articleService)
    {
        _articleService = articleService;
    }

    [HttpPost("FetchAiArticle")]
    [Authorize]
    public async Task<IActionResult> FetchAiArticle([FromBody] FetchAiArticleParams fetchAiArticleParams)
    {
        return new StreamedResult(async (outputStream, cancellationToken) =>
        {
            try
            {
                await _articleService.SteamFeatchAiArticle(outputStream, fetchAiArticleParams, cancellationToken);
            }
            finally
            {
                outputStream.Close();
            }
        }, "text/event-stream");
    }

    [HttpPost("VectorizeArticle")]
    [Authorize]
    public async Task<IActionResult> VectorizeArticle([FromBody] VectorizeArticleParams vectorizeArticleParams)
    {
        var result = await _articleService.VectorizeArticle(vectorizeArticleParams);

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpPost("GenerateArticle")]
    [Authorize]
    public async Task<IActionResult> GenerateArticle([FromBody] GenerateArticleParams generateArticleParams)
    {
        var result = await _articleService.GenerateArticle(generateArticleParams);

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpDelete("DeleteArticle/{articleId}")]
    [Authorize]
    public async Task<IActionResult> DeleteArticle(int articleId)
    {
        var result = await _articleService.DeleteArticle(articleId);
        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpGet("GetArticle/{articleId}")]
    [Authorize]
    public async Task<IActionResult> GetArticle(int articleId)
    {
        var result = await _articleService.GetArticle(articleId);

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpGet("GetArticleList")]
    [Authorize]
    public async Task<IActionResult> GetArticleList([FromQuery] SearchParams? searchParams = null)
    {
        var result = await _articleService.GetArticleList(searchParams);

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpPost("UpdateArticleReadingProgress")]
    [Authorize]
    public async Task<IActionResult> UpdateArticleReadingProgress(UpdateArticleReadingProgressParams progressParams)
    {
        var result = await _articleService.UpdateArticleReadingProgress(progressParams);

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }

    [HttpGet("GetArticleReadingProgress/{articleId}")]
    [Authorize]
    public async Task<IActionResult> GetArticleReadingProgress(int articleId)
    {
        var result = await _articleService.GetArticleReadingProgress(articleId);

        if (result.IsSuccess)
            return Ok(result);
        else
            return BadRequest(result);
    }
}
