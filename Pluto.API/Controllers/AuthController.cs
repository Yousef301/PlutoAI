using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Pluto.Application.DTOs.Auth;
using Pluto.Application.Services;
using Pluto.Application.Services.EntityServices.Interfaces.Auth;
using Pluto.Application.Services.SharedServices.Interfaces;
using Pluto.DAL.Exceptions;

namespace Pluto.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        IServiceManager serviceManager,
        IConfiguration configuration
    )
    {
        _serviceManager = serviceManager;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] SignInRequest request)
    {
        var response = await _serviceManager.AuthenticationService.SignInAsync(request);

        if (response.EmailConfirmed)
        {
            _serviceManager.AuthenticationService
                .SetTokenInsideCookie(new TokenDto(response.AccessToken, response.RefreshToken), HttpContext);
        }

        return Ok(new { EmailConfirmed = response.EmailConfirmed });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        _serviceManager.AuthenticationService.RemoveCookies(HttpContext);

        return Ok();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] SignUpRequest request)
    {
        var response = await _serviceManager.AuthenticationService
            .SignUpAsync(request);

        return Created(string.Empty, new { response.Email });
    }


    [HttpPost("send-confirmation-email")]
    public async Task<IActionResult> SendConfirmEmail([FromBody] EmailConfirmationRequest request)
    {
        await _serviceManager.AuthenticationService
            .SendConfirmationEmail(request);

        return Ok();
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
    {
        await _serviceManager.AuthenticationService
            .ConfirmEmail(token);

        var accountActivatedUrl = _configuration["AccountActivatedUrl"] ?? throw new
            InvalidConfigurationException("Account activated URL is not configured");

        return Redirect(accountActivatedUrl);
    }


    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        HttpContext.Request.Cookies.TryGetValue("accessToken", out var accessToken);
        HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken);

        await _serviceManager.TokenGeneratorService
            .RefreshTokenAsync(new TokenDto(accessToken, refreshToken));

        _serviceManager.AuthenticationService
            .SetTokenInsideCookie(new TokenDto(accessToken, refreshToken), HttpContext);

        return Ok();
    }

    [HttpGet("verify")]
    public async Task<IActionResult> Verify()
    {
        var token = Request.Cookies["accessToken"];
        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized();
        }

        try
        {
            var userInfo = await _serviceManager
                .TokenGeneratorService.GetTokenClaims(token);

            return Ok(userInfo);
        }
        catch
        {
            return Unauthorized();
        }
    }
}