using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models;

[Table("Chat_Session")]
public partial class ChatSession
{
    [Key]
    [Column("SessionID")]
    public int SessionId { get; set; }

    [StringLength(50)]
    public string? SessionName { get; set; }

    [StringLength(50)]
    public string UserId { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime UpdateTime { get; set; }
}
