using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pluto.API.Helpers.Interfaces;
using Pluto.Application.DTOs.Sessions;
using Pluto.Application.Services;
using Pluto.Application.Services.EntityServices.Interfaces;

namespace Pluto.API.Controllers;

[Authorize]
[ApiController]
[Route("api/sessions")]
public class SessionsController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly IUserContext _userContext;

    public SessionsController(
        IUserContext userContext,
        IServiceManager serviceManager
    )
    {
        _userContext = userContext;
        _serviceManager = serviceManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserSessionsAsync()
    {
        var sessions = await _serviceManager.SessionService
            .GetUserSessionsAsync(_userContext.Id);

        return Ok(sessions);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSessionAsync()
    {
        var createdSession = await _serviceManager.SessionService
            .CreateAsync(new CreateSessionRequest(_userContext.Id));

        return Created(string.Empty, new { createdSession.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateSessionTitleAsync(
        [FromBody] UpdateSessionTitleRequest request,
        int id
    )
    {
        request.UserId = _userContext.Id;
        request.Id = id;

        var updatedSession = await _serviceManager.SessionService
            .UpdateAsync(request);

        return Ok(updatedSession);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSessionAsync(int id)
    {
        await _serviceManager.SessionService.DeleteAsync(new DeleteSessionRequest(id, _userContext.Id));

        return NoContent();
    }
}