using Microsoft.AspNetCore.Mvc;
using MediatR;
using ChatbotAIService.Features.Messages.Commands;
using ChatbotAIService.Features.Conversations.Commands;
using ChatbotAIService.DTOs;

namespace ChatbotAIService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        //TODO: return more than a string -> message content and message id
        //TODO: use type delta and message
        // SSE 
        [HttpPost("stream")]
        public async Task<IActionResult> StreamChat([FromBody] StreamChatDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var command = new StreamChatCommand
                {
                    Message = dto.Message,
                    ConversationId = dto.ConversationId,
                };

                var streamResponse = await _mediator.Send(command, cancellationToken);

                Response.Headers.ContentType = "text/event-stream";
                Response.Headers.CacheControl = "no-cache";
                Response.Headers.Connection = "keep-alive";
                Response.Headers.AccessControlAllowOrigin = "*";

                await foreach (var chunk in streamResponse.WithCancellation(cancellationToken))
                {
                    if (!string.IsNullOrEmpty(chunk))
                    {
                        var data = $"data: {chunk}\n\n";
                        await Response.WriteAsync(data, cancellationToken);
                        await Response.Body.FlushAsync(cancellationToken);
                    }
                }

                await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);


                return new EmptyResult();
            }
            catch (OperationCanceledException)
            {
                return new EmptyResult();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("stop_conversation")]
        public async Task<IActionResult> StopConversation([FromBody] StopConversationDto dto)
        {
            try
            {
                var command = new StopConversationCommand
                {
                    ConversationId = dto.ConversationId
                };

                var success = await _mediator.Send(command);

                if (!success)
                {
                    return NotFound(new { error = "Conversation not found" });
                }

                return Ok(new { message = "Conversation stopped successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        //TODO: return more than a string -> message content and message id
        //TODO: use type delta and message
        //SSE
        [HttpPost("resume")]
        public async Task<IActionResult> ResumeStream([FromBody] ResumeStreamDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var command = new ResumeStreamCommand
                {
                    ConversationId = dto.ConversationId,
                    Message = dto.Message
                };

                var streamResponse = await _mediator.Send(command, cancellationToken);

                Response.Headers.ContentType = "text/event-stream";
                Response.Headers.CacheControl = "no-cache";
                Response.Headers.Connection = "keep-alive";
                Response.Headers.AccessControlAllowOrigin = "*";

                await foreach (var chunk in streamResponse.WithCancellation(cancellationToken))
                {
                    if (!string.IsNullOrEmpty(chunk))
                    {
                        var data = $"data: {chunk}\n\n";
                        await Response.WriteAsync(data, cancellationToken);
                        await Response.Body.FlushAsync(cancellationToken);
                    }
                }

                await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);


                return new EmptyResult();
            }
            catch (OperationCanceledException)
            {
                return new EmptyResult();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}