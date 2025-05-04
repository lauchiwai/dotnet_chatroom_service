using Common.Dto;
using Common.Params;
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

    [HttpPost("GenerateArticle")]
    //[Authorize]
    public async Task<IActionResult> GenerateArticle([FromBody] ArticleGenerationParams articleGenerationParams)
    {
        return new StreamedResult(async (outputStream, cancellationToken) =>
        {
            try
            {
                await _articleService.SteamGenerateArticle(outputStream, articleGenerationParams, cancellationToken);
            }
            finally
            {
                outputStream.Close();
            }
        }, "text/event-stream");
    }
}
