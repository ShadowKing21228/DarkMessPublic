using DarkMessServer.API.Models;
using DarkMessServer.Core.Security;
using DarkMessServer.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace DarkMessServer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IConfiguration configuration) : ControllerBase
{
    private readonly IConfiguration _configuration = configuration;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginModel user)
    {
        Console.WriteLine("Запрос входа принят:" + user);
        if (!await UserRepository.ValidateCredentialsAsync(user.Email, user.Password)) return Unauthorized();
        Console.WriteLine($"Пользователь {user.Email} успешно вошёл!" );
        return Ok(new { Token = JwtHandler.GenerateJwtToken(user) });
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterModel user)
    {
        Console.WriteLine("Запрос регистрации принят:" + user);
        await UserRepository.CreateNewUser(user.Username, user.Password, user.Email);
        Console.WriteLine($"Пользователь {user.Email} успешно добавлен!" );
        return Ok(new { Token = JwtHandler.GenerateJwtToken(user) });
    }
    [HttpPost("ping")]
    public IActionResult Ping() => Ok("pong");
}