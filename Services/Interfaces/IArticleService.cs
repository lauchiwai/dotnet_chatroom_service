using Common.Dto;
using Common.Params;

namespace Services.Interfaces;

public interface IArticleService
{
    public Task SteamGenerateArticle(Stream outputStream, ArticleGenerationParams articleGenerationParams, CancellationToken cancellationToken);
 
}
