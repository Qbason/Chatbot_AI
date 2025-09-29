using Microsoft.AspNetCore.Mvc;
using MediatR;
using ChatbotAIService.Features.Messages.Commands;
using ChatbotAIService.Features.Conversations.Commands;
using ChatbotAIService.DTOs;
using ChatbotAIService.Features.Conversations.Queries;
using System.Text.Json;

namespace ChatbotAIService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

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

                var commandGetLastAIMessage = new GetLastAIMessageQuery
                {
                    ConversationId = dto.ConversationId,
                };
                var lastAIMessage = await _mediator.Send(commandGetLastAIMessage, cancellationToken);
                if (lastAIMessage == null)
                {
                    return StatusCode(500, new { error = "Failed to retrieve last AI message." });
                }

                Response.Headers.ContentType = "text/event-stream";
                Response.Headers.CacheControl = "no-cache";
                Response.Headers.Connection = "keep-alive";
                Response.Headers.AccessControlAllowOrigin = "*";

                var messageIdJson = JsonSerializer.Serialize(new { type = "messageId", messageId = lastAIMessage.Id });
                var messageIdData = $"data: {messageIdJson}\n\n";
                await Response.WriteAsync(messageIdData, cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);

                await foreach (var chunk in streamResponse.WithCancellation(cancellationToken))
                {
                    if (!string.IsNullOrEmpty(chunk))
                    {
                        var jsonData = JsonSerializer.Serialize(new { type = "v", content = chunk });
                        var data = $"data: {jsonData}\n\n";
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
    }
}