namespace Common.ViewModels.Word;

public class WordViewModel
{
    /// <summary>
    /// 關聯編號
    /// </summary>
    public int UserWordId { get; set; }

    /// <summary>
    /// 單字編號
    /// </summary>
    public int WordId { get; set; }

    /// <summary>
    /// 單字
    /// </summary>
    public string Word { get; set; }

    /// <summary>
    /// 下一次復習時間
    /// </summary>
    public DateTime NextReviewDate { get; set; }

    /// <summary>
    /// 上一次復習時間
    /// </summary>
    public DateTime? LastReviewed { get; set; }
 
    /// <summary>
    /// 復習次數
    /// </summary>
    public int ReviewCount { get; set; }
}
