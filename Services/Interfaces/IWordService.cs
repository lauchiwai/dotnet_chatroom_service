using Common.Dto;
using Common.Params.Word;

namespace Services.Interfaces;

public interface IWordService
{
    /// <summary>
    /// 取得當前使用者的單字列表
    /// </summary>
    Task<ResultDTO> GetWordList();

    /// <summary>
    /// 根據單字ID取得單字詳情
    /// </summary>
    /// <param name="wordId">單字ID</param>
    Task<ResultDTO> GetWordById(int wordId);

    /// <summary>
    /// 新增單字到使用者詞庫
    /// </summary>
    /// <param name="addParams">新增單字參數</param>
    Task<ResultDTO> AddWord(AddWordParams addParams);

    /// <summary>
    /// 更新單字複習狀態
    /// </summary>
    /// <param name="wordId"></param>
    /// <returns></returns>
    Task<ResultDTO> UpdateWordReviewStatus(int wordId);

    /// <summary>
    /// 從使用者詞庫中移除單字
    /// </summary>
    /// <param name="wordId">單字ID</param>
    Task<ResultDTO> RemoveWordById(int wordId);

    /// <summary>
    /// 從使用者詞庫中移除單字
    /// </summary>
    /// <param name="word">單字ID</param>
    Task<ResultDTO> RemoveWordByText(string word);

    /// <summary>
    /// 檢查單字是否存在於使用者詞庫
    /// </summary>
    /// <param name="wordId">單字ID</param>
    Task<ResultDTO> CheckUserWordExistsById(int wordId);

    /// <summary>
    /// 檢查單字是否存在於使用者詞庫
    /// </summary>
    /// <param name="word">單字</param>
    Task<ResultDTO> CheckUserWordExistsByText(string word);
}
