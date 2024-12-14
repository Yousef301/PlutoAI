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
    private readonly IEmailService _emailService;
    private readonly IGoogleAuthService _googleAuthService;

    public AuthController(
        IUserService userService,
        IGoogleAuthService googleAuthService,
        IEmailService emailService
    )
    {
        _userService = userService;
        _emailService = emailService;
        _googleAuthService = googleAuthService;
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

        return Ok();
    }
}