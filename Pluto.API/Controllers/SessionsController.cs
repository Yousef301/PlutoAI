using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pluto.API.Helpers.Interfaces;
using Pluto.Application.DTOs.Sessions;
using Pluto.Application.Services.EntityServices.Interfaces;

namespace Pluto.API.Controllers;

[Authorize]
[ApiController]
[Route("api/sessions")]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IUserContext _userContext;

    public SessionsController(
        ISessionService sessionService,
        IUserContext userContext
    )
    {
        _sessionService = sessionService;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserSessionsAsync()
    {
        var sessions = await _sessionService
            .GetUserSessionsAsync(_userContext.Id);

        return Ok(sessions);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSessionAsync()
    {
        var createdSession = await _sessionService
            .CreateAsync(new CreateSessionRequest(_userContext.Id));

        return Created(string.Empty, new { createdSession.Id });
    }
}