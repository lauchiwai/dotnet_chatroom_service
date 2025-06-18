using Common.Dto;
using Common.Params.Vector;
using Common.ViewModels.Article;
using MediatR;
using Services.Interfaces;
using System.Text;
using System.Text.RegularExpressions;

public class VectorizeArticleCommandHandler : IRequestHandler<VectorizeArticleCommand, ResultDTO>
{
    private readonly IVectorService _vectorService;
    private readonly IArticleService _articleService;

    public VectorizeArticleCommandHandler(
        IVectorService vectorService,
        IArticleService articleService)
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

            if (articleResult.Data is not ArticleViewModel article)
            {
                return new ResultDTO
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = "Invalid article data type"
                };
            }

            var cleanContent = Regex.Replace(article.ArticleContent, "<.*?>", string.Empty);

            var rawParagraphs = cleanContent
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .ToList();

            const int minWordCount = 500; 
            var mergedParagraphs = new List<string>();
            var currentBlock = new StringBuilder();
            int currentWordCount = 0;

            foreach (var paragraph in rawParagraphs)
            {
                int paragraphWordCount = CountWords(paragraph);

                if (currentBlock.Length > 0)
                {
                    currentBlock.Append('\n');
                }
                currentBlock.Append(paragraph);
                currentWordCount += paragraphWordCount;

                if (currentWordCount >= minWordCount)
                {
                    mergedParagraphs.Add(currentBlock.ToString());
                    currentBlock.Clear();
                    currentWordCount = 0;
                }
            }

            if (currentBlock.Length > 0)
            {
                mergedParagraphs.Add(currentBlock.ToString());
            }

            var textPoints = mergedParagraphs
                .Select((p, index) => new TextPoint
                {
                    Text = p,
                    Id = (int)request.ArticleId + 10000 + index
                })
                .ToList();

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
                Code = 500,
                Message = "Article processing error. Please try again later."
            };
        }
    }

    private static int CountWords(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return 0;

        int wordCount = 0;
        bool isInWord = false;

        foreach (char c in input)
        {
            if (char.IsWhiteSpace(c) || char.IsPunctuation(c))
            {
                isInWord = false;
            }
            else if (!isInWord)
            {
                wordCount++;
                isInWord = true;
            }
        }

        return wordCount;
    }
}
