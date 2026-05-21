using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ReddIF.Models;

[Table("post_votes")]
public class PostVote : BaseModel
{
    [PrimaryKey("post_vote_id", false)]
    public long PostVoteId { get; set; }

    [Column("post_id")]
    public int PostId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("vote_value")]
    public int VoteValue { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}