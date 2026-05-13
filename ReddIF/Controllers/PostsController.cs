using Microsoft.AspNetCore.Mvc;
using ReddIF.Models;
using Supabase;

namespace ReddIF.Controllers;

[ApiController]
[Route("api/communities/{communityId:int}/posts")]
public class PostsController: ControllerBase
{
    private readonly Client _supabase;

    public PostsController(Client supabase)
    {
        _supabase = supabase;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePost(
        [FromRoute] int communityId,
        [FromBody] PostForm req)
    {
    var post = new Post
    {
        Title = req.Title,
        Content = req.Content,
        CommunityId = communityId
    };

    var response = await _supabase.From<Post>().Insert(post);
    if (response.Models == null || !response.Models.Any())    {
        return BadRequest( new { error = "Erro ao criar post" });
    }   
    return Ok(post);
}

[HttpGet]
public async Task<IActionResult> GetPosts(int communityId)
{
    var response = await _supabase
        .From<Post>()
        .Where(p => p.CommunityId == communityId)
        .Get();

    return Ok(response.Models);
}

    public record PostForm(string Title, string Content);
}