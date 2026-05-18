using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ReddIF.Models;

[Table("communities")]
public class Community : BaseModel
{
    [PrimaryKey("community_id", false)]
    public int CommunityId { get; set; }

    [Column("owner")]
    public int OwnerId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}