using Microsoft.AspNetCore.Mvc;
using Pluto.API.Helpers.Interfaces;
using Pluto.Application.DTOs.Auth;
using Pluto.Application.Services.EntityServices.Interfaces.Auth;

namespace Pluto.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IUserContext _userContextAccessor;

    public AuthController(
        IUserService userService,
        IGoogleAuthService googleAuthService, IUserContext userContextAccessor)
    {
        _userService = userService;
        _googleAuthService = googleAuthService;
        _userContextAccessor = userContextAccessor;
    }

    [HttpPost("signin")]
    public async Task<IActionResult> Login([FromBody] SignInRequest request)
    {
        var response = await _userService.SignInAsync(request);

        Console.WriteLine(response.Token);

        return Ok(response.Token);
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

        return Content($@"
        <script>
            window.opener.postMessage({{ type: 'google-auth-success', token: '{token}' }}, '*');
            window.close();
        </script>
    ", "text/html");
    }
}