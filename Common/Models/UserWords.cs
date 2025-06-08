namespace Common.Models;

public partial class UserWords
{
    public int UserWordId { get; set; }

    public int UserId { get; set; }

    public int WordId { get; set; }

    public DateTime NextReviewDate { get; set; }

    public DateTime? LastReviewed { get; set; }

    public int ReviewCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Authenticate User { get; set; } = null!;

    public virtual Words Word { get; set; } = null!;
}
