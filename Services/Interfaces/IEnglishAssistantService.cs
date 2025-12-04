using Common.Params.EnglishAssistant;

namespace Services.Interfaces;

public interface IEnglishAssistantService
{
    /// <summary>
    /// AI 單字翻譯
    /// </summary>
    /// <param name="outputStream"></param>
    /// <param name="param"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task WordTranslate(Stream outputStream, WordAssistanParams param, CancellationToken cancellationToken);
    
    /// <summary>
    /// AI 單字解析 
    /// </summary>
    /// <param name="outputStream"></param>
    /// <param name="param"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task WordAssistan(Stream outputStream, WordAssistanParams param, CancellationToken cancellationToken);

    /// <summary>
    /// AI 英文段落分析
    /// </summary>
    /// <param name="outputStream"></param>
    /// <param name="param"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task TextLinguisticAssistant(Stream outputStream, TextLinguisticAssistantParams param, CancellationToken cancellationToken);
}
