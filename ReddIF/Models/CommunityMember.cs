using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ReddIF.Models;

[Table("community_members")]
public class CommunityMember : BaseModel
{
    [PrimaryKey("community_member_id", false)]
    public int CommunityMemberId { get; set; }

    [Column("community_id")]
    public int CommunityId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("joined_at")]
    public DateTime JoinedAt { get; set; } = DateTime.Now;
}