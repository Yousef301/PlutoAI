using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Pluto.Application.DTOs.Auth;
using Pluto.Application.Services.EntityServices.Interfaces.Auth;
using Pluto.Application.Services.SharedServices.Interfaces;
using Pluto.DAL.Exceptions;

namespace Pluto.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IConfiguration _configuration;
    private readonly ITokenGeneratorService _tokenGeneratorService;

    public AuthController(
        IAuthenticationService authenticationService,
        IGoogleAuthService googleAuthService,
        IConfiguration configuration,
        ITokenGeneratorService tokenGeneratorService
    )
    {
        _authenticationService = authenticationService;
        _googleAuthService = googleAuthService;
        _configuration = configuration;
        _tokenGeneratorService = tokenGeneratorService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] SignInRequest request)
    {
        var response = await _authenticationService.SignInAsync(request);

        if (response.EmailConfirmed)
        {
            _authenticationService
                .SetTokenInsideCookie(new TokenDto(response.AccessToken, response.RefreshToken), HttpContext);
        }

        return Ok(new { EmailConfirmed = response.EmailConfirmed });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        _authenticationService.RemoveCookies(HttpContext);

        return Ok();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] SignUpRequest request)
    {
        var response = await _authenticationService.SignUpAsync(request);

        return Created(string.Empty, new { response.Email });
    }

    [HttpGet("google-login")]
    public IActionResult RedirectToGoogle()
    {
        var redirectUrl = _googleAuthService.GetGoogleOAuthUrl();
        return Redirect(redirectUrl);
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return Content(@"
            <script>
                window.opener.postMessage({ type: 'google-auth-error' }, '*');
                window.close();
            </script>
        ", "text/html");
        }

        var tokens = await _googleAuthService.HandleGoogleCallbackAsync(code);
        if (tokens.AccessToken == null)
        {
            return Content(@"
            <script>
                window.opener.postMessage({ type: 'google-auth-error' }, '*');
                window.close();
            </script>
        ", "text/html");
        }

        _authenticationService
            .SetTokenInsideCookie(new TokenDto(tokens.AccessToken, tokens.RefreshToken), HttpContext);

        return Content(@"
        <script>
            window.opener.postMessage({ type: 'google-auth-success' }, '*');
            window.close();
        </script>
    ", "text/html");
    }

    [HttpPost("send-confirmation-email")]
    public async Task<IActionResult> SendConfirmEmail([FromBody] EmailConfirmationRequest request)
    {
        await _authenticationService.SendConfirmationEmail(request);

        return Ok();
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
    {
        await _authenticationService.ConfirmEmail(token);

        var accountActivatedUrl = _configuration["AccountActivatedUrl"] ?? throw new
            InvalidConfigurationException("Account activated URL is not configured");

        return Redirect(accountActivatedUrl);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] SendPasswordResetRequest request)
    {
        await _authenticationService.SendPasswordResetEmail(request);

        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _authenticationService.ResetPassword(request);

        return Ok();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        HttpContext.Request.Cookies.TryGetValue("accessToken", out var accessToken);
        HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken);

        await _tokenGeneratorService
            .RefreshTokenAsync(new TokenDto(accessToken, refreshToken));

        _authenticationService
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
            var userInfo = await _tokenGeneratorService.GetTokenClaims(token);

            return Ok(userInfo);
        }
        catch
        {
            return Unauthorized();
        }
    }
}