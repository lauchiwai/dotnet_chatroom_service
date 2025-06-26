using Common.Dto;
using Common.Params.Article;
using Common.Params.Search;

namespace Services.Interfaces;

public interface IArticleService
{
    /// <summary>
    /// ai 創建文章
    /// </summary>
    /// <param name="outputStream"></param>
    /// <param name="fetchAiArticleParams"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task SteamFeatchAiArticle(Stream outputStream, FetchAiArticleParams fetchAiArticleParams, CancellationToken cancellationToken);
    
    /// <summary>
    /// 創建文章資料
    /// </summary>
    /// <param name="generateArticleParams"></param>
    /// <returns></returns>
    public Task<ResultDTO> GenerateArticle(GenerateArticleParams generateArticleParams);

    /// <summary>
    /// 刪除文章資料
    /// </summary>
    /// <param name="articleId"></param>
    /// <returns></returns>
    public Task<ResultDTO> DeleteArticle(int articleId);

    /// <summary>
    /// 獲取 刪除文章的資料
    /// </summary>
    /// <param name="articleId"></param>
    /// <returns></returns>
    public Task<ResultDTO> RequestArticleDeletion(int articleId);

    /// <summary>
    ///  文章向量化
    /// </summary>
    /// <param name="vectorizeArticleParams"></param>
    /// <returns></returns>
    public Task<ResultDTO> VectorizeArticle(VectorizeArticleParams vectorizeArticleParams);

    /// <summary>
    /// 獲取指定文章内容
    /// </summary>
    /// <param name="articleId"></param>
    /// <returns></returns>
    public Task<ResultDTO> GetArticle(int articleId);

    /// <summary>
    /// 獲取所有文章内容
    /// </summary>
    /// <returns></returns>
    public Task<ResultDTO> GetArticleList(SearchParams? searchParams = null);

    /// <summary>
    /// 更新文章觀看進度
    /// </summary>
    /// <param name="progressParams"></param>
    /// <returns></returns>
    public Task<ResultDTO> UpdateArticleReadingProgress(UpdateArticleReadingProgressParams progressParams);

    /// <summary>
    /// 獲取文章觀看進度
    /// </summary>
    /// <param name="articleId"></param>
    /// <returns></returns>
    public Task<ResultDTO> GetArticleReadingProgress(int articleId);
}
