using Microsoft.AspNetCore.Mvc;
using ReddIF.Models;
using Supabase;

namespace ReddIF.Controllers;


[ApiController]
[Route("api/home")]
public class TesteController : ControllerBase
{
    private readonly Client _supabase;

    public TesteController(Client supabase)
    {
        _supabase = supabase;
    }

    [HttpGet]
    public async Task<IActionResult> Testar()
    {
        
        
        
        try
        {
            var response = await _supabase
                .From<User>()
                .Get();

            var usuarios = response.Models.Select(u => new
            {
                u.UserId,
                u.Name,
                u.Email,
                u.Karma,
                u.Active
            });

            return Ok(new
            {
                total = usuarios.Count(),
                usuarios
            });        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                erro = ex.Message
            });
        }
    }
}