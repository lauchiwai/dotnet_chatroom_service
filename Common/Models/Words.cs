namespace Common.Models;
public partial class Words
{
    public int WordId { get; set; }

    public string Word { get; set; } = null!;

    public DateTime AddedAt { get; set; }

    public virtual ICollection<UserWords> UserWords { get; set; } = new List<UserWords>();
}
