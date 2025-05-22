using Common.Dto;
using Common.Params.Vector;
using Common.ViewModels.Article;
using MediatR;
using Services.Interfaces;

public class VectorizeArticleCommandHandler : IRequestHandler<VectorizeArticleCommand, ResultDTO>
{
    private readonly IVectorService _vectorService;
    private readonly IArticleService _articleService;

    public VectorizeArticleCommandHandler(IVectorService vectorService, IArticleService articleService)
    {
        _vectorService = vectorService;
        _articleService = articleService;
    }

    public async Task<ResultDTO> Handle(VectorizeArticleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var articleResult = await _articleService.GetArticle(request.ArticleId);
            if (!articleResult.IsSuccess)
            {
                return articleResult;
            }

            var article = articleResult.Data as ArticleViewModel;

            var paragraphs = article.ArticleContent
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            var textPoints = paragraphs.Select((p, index) => new TextPoint
            {
                Text = p.Trim(),
                Id = request.ArticleId * 1000 + index 
            }).ToList();

            var vectorParams = new UpsertVectorCollectionParams
            {
                Id = request.ArticleId,
                CollectionName = request.CollectionName,
                Points = textPoints
            };

            var vectorResult = await _vectorService.UpsertVectorCollectionTexts(vectorParams);

            return new ResultDTO
            {
                IsSuccess = vectorResult.IsSuccess,
                Code = vectorResult.Code,
                Data = vectorResult.Data,
                Message = vectorResult.Message
            };
        }
        catch (Exception ex)
        {
            return new ResultDTO
            {
                IsSuccess = false,
                Code = 400,
                Message = $"Vectorization failed: {ex.Message}"
            };
        }
    }
}