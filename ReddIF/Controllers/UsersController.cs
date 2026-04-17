using Microsoft.AspNetCore.Mvc;
using ReddIF.Models;
using ReddIF.Context;
using Microsoft.EntityFrameworkCore;

namespace ReddIF.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController: ControllerBase
{
    private readonly AppDbContext _context;

}