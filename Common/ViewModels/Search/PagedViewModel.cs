namespace Common.ViewModels.Search;

public class PagedViewModel<T>
{
    /// <summary>
    /// 當前頁的數據項目列表
    /// </summary>
    public List<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// 符合條件的總記錄數（不分頁）
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 當前頁碼（從1開始）
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// 每頁顯示的記錄數量
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 總頁數（根據TotalCount和PageSize計算）
    /// </summary>
    public int TotalPages { get; set; }
}