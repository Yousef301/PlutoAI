using Microsoft.AspNetCore.Mvc;
using Pluto.Application.DTOs.Auth;
using Pluto.Application.Services.Interfaces;

namespace Pluto.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("signin")]
    public async Task<IActionResult> Login([FromBody] SignInRequest request)
    {
        var response = await _userService.SignInAsync(request);

        return Ok(response.Token);
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Register([FromBody] SignUpRequest request)
    {
        var response = await _userService.SignUpAsync(request);

        return Created(string.Empty, new { response.Email });
    }
}