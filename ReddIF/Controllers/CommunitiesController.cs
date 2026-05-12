using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReddIF.Models;
using Supabase;
using System.Security.Claims;

namespace ReddIF.Controllers;

[ApiController]
[Route("api/communities")]
public class CommunitiesController : ControllerBase
{
    private readonly Client _supabase;

    public CommunitiesController(Client supabase)
    {
        _supabase = supabase;
    }


    // [Authorize] = Precisa de autenticação. A criação É permitida a qualquer usuário autenticado
    // Edição e exclusão são restritas ao proprietário da comunidade ou administrador


    // POST /api/communities — Cria comunidade
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CommunityForm req)
    {
        try
        {
            // Pega o id do usuário logado pelo token
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return Unauthorized();
            var userId = int.Parse(userIdStr);

            var existe = await _supabase
                .From<Community>()
                .Where(c => c.Nome == req.Nome)
                .Get();

            if (existe.Models.Any())
                return BadRequest(new { erro = "Já existe uma comunidade com esse nome" });


            
             // cria a nova comunidade com o id do usuário logado como dono
            var novaComunidade = new Community
            {
                Nome = req.Nome,
                Description = req.Description ?? string.Empty,
                OwnerId = userId,
                CreateTime = DateTime.Now
            };

            var response = await _supabase.From<Community>().Insert(novaComunidade);
            var comunidade = response.Models.FirstOrDefault();

            return Ok(new
            {
                mensagem = "Comunidade criada com sucesso!",
                comunidade?.CommunityId,
                comunidade?.Nome,
                comunidade?.Description
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = ex.Message });
        }
    }


    // GET /api/communities — Lista todas comunidades
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var response = await _supabase.From<Community>().Get();

            var comunidades = response.Models.Select(c => new
            {
                c.CommunityId,
                c.Nome,
                c.Description,
                c.OwnerId,
                c.CreateTime
            });

            return Ok(new { total = comunidades.Count(), comunidades });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = ex.Message });
        }
    }



    // GET /api/communities/{nome} — Busca comunidade por nome
    [HttpGet("{nome}")]
    public async Task<IActionResult> GetByNome(string nome)
    {
        try
        {
            var response = await _supabase
                .From<Community>()
                .Where(c => c.Nome == nome)
                .Get();

            var comunidade = response.Models.FirstOrDefault();

            if (comunidade == null)
                return NotFound(new { erro = "Comunidade não encontrada" });

            return Ok(new
            {
                comunidade.CommunityId,
                comunidade.Nome,
                comunidade.Description,
                comunidade.OwnerId,
                comunidade.CreateTime
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = ex.Message });
        }
    }



    // PUT /api/communities/{id} — Edita uma comunidade
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCommunityForm req)
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userIdStr == null) return Unauthorized();
            var userId = int.Parse(userIdStr);

            var response = await _supabase
                .From<Community>()
                .Where(c => c.CommunityId == id)
                .Get();

            var comunidade = response.Models.FirstOrDefault();
            if (comunidade == null)
                return NotFound(new { erro = "Comunidade não encontrada" });


            if (comunidade.OwnerId != userId && userRole != "admin")
                return Forbid();

            if (!string.IsNullOrEmpty(req.Nome))
                comunidade.Nome = req.Nome;

            if (!string.IsNullOrEmpty(req.Description))
                comunidade.Description = req.Description;

            await _supabase.From<Community>().Update(comunidade);

            return Ok(new { mensagem = "Comunidade atualizada com sucesso!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = ex.Message });
        }
    }



    // DELETE /api/communities/{id} — Exclui uma comunidade
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userIdStr == null) return Unauthorized();
            var userId = int.Parse(userIdStr);

            var response = await _supabase
                .From<Community>()
                .Where(c => c.CommunityId == id)
                .Get();

            var comunidade = response.Models.FirstOrDefault();
            if (comunidade == null)
                return NotFound(new { erro = "Comunidade não encontrada" });

            if (comunidade.OwnerId != userId && userRole != "admin")
                return Forbid();

            await _supabase
                .From<Community>()
                .Where(c => c.CommunityId == id)
                .Delete();

            return Ok(new { mensagem = "Comunidade deletada com sucesso!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = ex.Message });
        }
    }
}

public record CommunityForm(string Nome, string? Description);
public record UpdateCommunityForm(string? Nome, string? Description);