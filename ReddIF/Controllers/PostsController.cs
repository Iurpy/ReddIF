using Microsoft.AspNetCore.Mvc;
using ReddIF.Models;
using Supabase;

namespace ReddIF.Controllers;

[ApiController]
[Route("posts")]
public class PostsController: ControllerBase
{
    private readonly Client _supabase;

    public PostsController(Client supabase)
    {
        _supabase = supabase;
    }

    [HttpPost]
    public async Task<IActionResult> PostPost([FromBody] PostForm req)
    {

        var novoPost = new Post
        {
            Title = req.Title,
            Content = req.Content,
            UserAutorId = 16//tem q pegar o id pelo token, ainda n sei fzr :(
        };

        var response = await _supabase.From<Post>().Insert(novoPost);
        var post = response.Models.FirstOrDefault();
        
        return Ok(new
        {
            mensagem = "Post criado com sucesso",
            post.Title,
            post.Content
        });


    }


    public record PostForm(string Title, string Content);
}