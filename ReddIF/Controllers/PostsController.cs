using Microsoft.AspNetCore.Mvc;
using ReddIF.Models;
using Supabase;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
[Authorize]
public async Task<IActionResult> CreatePost(
int communityId,
[FromBody] PostForm req)
{
var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

if (userIdStr == null)
    return Unauthorized();

var userId = int.Parse(userIdStr);

var post = new Post
{
    Title = req.Title,
    Content = req.Content,
    CommunityId = communityId,
    UserAutorId = userId
};

var response = await _supabase
    .From<Post>()
    .Insert(post);

var createdPost = response.Models.First();

return Ok(new
{
    createdPost.PostId,
    createdPost.Title,
    createdPost.Content,
    createdPost.CommunityId,
    createdPost.UserAutorId
});
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


[HttpDelete("{postId:int}")]
public async Task<IActionResult> DeletePost(int postId)
{
    var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdStr == null) return Unauthorized();
    var userId = int.Parse(userIdStr);

    var response = await _supabase
        .From<Post>()
        .Where(p => p.PostId == postId)
        .Get();

    var post = response.Models.FirstOrDefault();
    if (post == null) return NotFound(new { erro = "Post não encontrado" });

    if (post.UserAutorId != userId)
        return Forbid();

    await _supabase
        .From<Post>()
        .Where(p => p.PostId == postId)
        .Delete();

    return Ok(new { message = "Post deletado com sucesso" });
}

    public record PostForm(string Title, string Content);
}
