using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pluto.API.Helpers.Interfaces;
using Pluto.Application.DTOs.Messages;
using Pluto.Application.Services.EntityServices.Interfaces;

namespace Pluto.API.Controllers;

[Authorize]
[ApiController]
[Route("api/sessions/{sessionId:int}/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IUserContext _userContext;

    public MessagesController(
        IMessageService messageService,
        IUserContext userContext
    )
    {
        _messageService = messageService;
        _userContext = userContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetSessionMessagesAsync(int sessionId)
    {
        var request = new GetMessagesRequest
        {
            SessionId = sessionId,
            UserId = _userContext.Id
        };

        var messages = await _messageService.GetSessionMessagesAsync(request);

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

        var message = await _messageService.SendMessageAsync(request);

        return Created(string.Empty, message);
    }
}