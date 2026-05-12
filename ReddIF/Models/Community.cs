using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ReddIF.Models;

[Table("community")]
public class Community : BaseModel
{
    [PrimaryKey("communityid", false)]
    public int CommunityId { get; set; }

    [Column("dono")]
    public int OwnerId { get; set; }

    [Column("nome")]
    public string Nome { get; set; } = string.Empty;

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("createtime")]
    public DateTime CreateTime { get; set; } = DateTime.Now;
}