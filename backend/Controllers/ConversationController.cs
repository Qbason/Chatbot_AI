using Microsoft.AspNetCore.Mvc;
using ChatbotAIService.Models;
using MediatR;
using ChatbotAIService.Features.Conversations.Commands;
using ChatbotAIService.Features.Conversations.Queries;
using ChatbotAIService.Features.Messages.Commands;
using ChatbotAIService.DTOs;

namespace ChatbotAIService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ConversationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Conversation>>> GetConversations()
        {
            var conversations = await _mediator.Send(new GetAllConversationsQuery());
            return Ok(conversations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Conversation>> GetConversation(int id)
        {
            var conversation = await _mediator.Send(new GetConversationByIdQuery
            {
                Id = id
            });

            if (conversation == null)
            {
                return NotFound();
            }

            return Ok(conversation);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutConversation(int id, UpdateConversationDto dto)
        {
            var command = new UpdateConversationCommand
            {
                Id = id,
                Title = dto.Title
            };

            var success = await _mediator.Send(command);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Conversation>> PostConversation(CreateConversationDto dto)
        {
            var command = new CreateConversationCommand
            {
                Title = dto.Title
            };

            var conversation = await _mediator.Send(command);

            return CreatedAtAction("GetConversation", new { id = conversation.Id }, conversation);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConversation(int id)
        {
            var success = await _mediator.Send(new DeleteConversationCommand{ Id = id });

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("{id}/stream_status")]
        public async Task<ActionResult<ConversationStreamStatusDto>> GetConversationStreamStatus(int id)
        {
            var status = await _mediator.Send(new GetConversationStreamStatusQuery{ ConversationId = id });

            if (status == null)
            {
                return NotFound();
            }

            return Ok(new ConversationStreamStatusDto { Status = status });
        }

        [HttpPost("message_feedback")]
        public async Task<IActionResult> AddMessageFeedback([FromBody] MessageFeedbackDto dto)
        {
            var command = new AddMessageFeedbackCommand
            {
                ConversationId = dto.ConversationId,
                MessageId = dto.MessageId,
                Rating = dto.Rating
            };

            var success = await _mediator.Send(command);

            if (!success)
            {
                return BadRequest("Message not found or does not belong to the specified conversation");
            }

            return Ok(new { message = "Feedback added successfully" });
        }
    }
}
