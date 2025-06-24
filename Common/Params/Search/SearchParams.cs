namespace Common.Params.Search;

public class SearchParams
{
    private const int MaxPageSize = 100;  
    private int _pageSize = 10;          

    /// <summary>
    /// 當前頁碼（從1開始）
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// 每頁記錄數（預設10，最大100）
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    /// <summary>
    /// 關鍵詞搜索
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 排序字段
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// 排序方向（asc/desc，預設asc）
    /// </summary>
    public string? SortDirection { get; set; } = "asc";

    /// <summary>
    /// 起始時間（用於時間範圍過濾）
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 結束時間（用於時間範圍過濾）
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 自定義過濾條件（鍵值對字典）
    /// </summary>
    public Dictionary<string, object>? CustomFilters { get; set; }
}
