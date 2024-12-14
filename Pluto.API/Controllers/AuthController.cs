using Microsoft.AspNetCore.Mvc;
using Pluto.API.Helpers.Interfaces;
using Pluto.Application.DTOs.Auth;
using Pluto.Application.Services.EntityServices.Interfaces.Auth;
using Pluto.Application.Services.SharedServices.Interfaces;

namespace Pluto.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IConfiguration _configuration;

    public AuthController(
        IUserService userService,
        IGoogleAuthService googleAuthService,
        IConfiguration configuration
    )
    {
        _userService = userService;
        _googleAuthService = googleAuthService;
        _configuration = configuration;
    }

    [HttpPost("signin")]
    public async Task<IActionResult> Login([FromBody] SignInRequest request)
    {
        var response = await _userService.SignInAsync(request);

        return Ok(response);
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Register([FromBody] SignUpRequest request)
    {
        var response = await _userService.SignUpAsync(request);

        return Created(string.Empty, new { response.Email });
    }

    [HttpGet("google-signin")]
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

        var token = await _googleAuthService.HandleGoogleCallbackAsync(code);
        if (token == null)
        {
            return Content(@"
            <script>
                window.opener.postMessage({ type: 'google-auth-error' }, '*');
                window.close();
            </script>
        ", "text/html");
        }

        Console.WriteLine(token);

        return Content($@"
        <script>
            window.opener.postMessage({{ type: 'google-auth-success', token: '{token}' }}, '*');
            window.close();
        </script>
    ", "text/html");
    }

    [HttpPost("send-confirmation-email")]
    public async Task<IActionResult> SendConfirmEmail([FromBody] EmailConfirmationRequest request)
    {
        await _userService.SendConfirmationEmail(request);

        return Ok();
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
    {
        await _userService.ConfirmEmail(token);

        return Redirect(_configuration["AccountActivatedUrl"]);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] SendPasswordResetRequest request)
    {
        await _userService.SendPasswordResetEmail(request);

        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _userService.ResetPassword(request);

        return Ok();
    }
}