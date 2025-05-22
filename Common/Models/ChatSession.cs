using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models;

[Table("Chat_Session")]
public partial class ChatSession
{
    /// <summary>
    /// 聊天室編號
    /// </summary>
    [Key]
    [Column("SessionID")]
    public int SessionId { get; set; }

    /// <summary>
    /// 聊天室名稱
    /// </summary>
    [StringLength(50)]
    public string? SessionName { get; set; }

    /// <summary>
    /// 使用者編號
    /// </summary>
    [StringLength(50)]
    public string UserId { get; set; } = null!;

    /// <summary>
    /// 更新時間
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime UpdateTime { get; set; }
}
