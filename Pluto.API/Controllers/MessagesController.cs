using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pluto.API.Helpers.Interfaces;
using Pluto.Application.DTOs.Messages;
using Pluto.Application.Services;
using Pluto.Application.Services.EntityServices.Interfaces;

namespace Pluto.API.Controllers;

[Authorize]
[ApiController]
[Route("api/sessions/{sessionId:int}/messages")]
public class MessagesController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly IUserContext _userContext;

    public MessagesController(
        IUserContext userContext,
        IServiceManager serviceManager
    )
    {
        _userContext = userContext;
        _serviceManager = serviceManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetSessionMessagesAsync(int sessionId)
    {
        var request = new GetMessagesRequest
        {
            SessionId = sessionId,
            UserId = _userContext.Id
        };

        var messages = await _serviceManager.MessageService
            .GetSessionMessagesAsync(request);

        return Ok(messages);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMessageAsync(
        int sessionId,
        CreateMessageRequest request
    )
    {
        request.SessionId = sessionId;
        request.UserId = _userContext.Id;

        var message = await _serviceManager.MessageService.SendMessageAsync(request);

        return Created(string.Empty, message);
    }
}