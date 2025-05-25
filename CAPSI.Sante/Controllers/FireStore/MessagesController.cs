using CAPSI.Sante.Domain.Models.Firestore;
using CAPSI.Sante.Infrastructure.Firebase.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CAPSI.Sante.API.Controllers.FireStore
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly MessageService _messageService;

        public MessagesController(MessageService messageService)
        {
            _messageService = messageService;
        }

        // POST: api/Messages
        [HttpPost]
        public async Task<ActionResult<string>> EnvoyerMessage(MessageTempsReel message)
        {
            var id = await _messageService.EnvoyerMessageAsync(message);
            return CreatedAtAction(nameof(GetMessagesConversation), new { conversationId = message.ConversationId }, id);
        }

        // GET: api/Messages/conversation/{conversationId}
        [HttpGet("conversation/{conversationId}")]
        public async Task<ActionResult<List<MessageTempsReel>>> GetMessagesConversation(string conversationId)
        {
            var messages = await _messageService.GetMessagesConversationAsync(conversationId);
            return messages;
        }

        // PUT: api/Messages/{messageId}/recu
        [HttpPut("{messageId}/recu")]
        public async Task<IActionResult> MarquerCommeRecu(string messageId)
        {
            await _messageService.MarquerCommeRecuAsync(messageId);
            return NoContent();
        }

        // PUT: api/Messages/{messageId}/lu
        [HttpPut("{messageId}/lu")]
        public async Task<IActionResult> MarquerCommeLu(string messageId)
        {
            await _messageService.MarquerCommeLuAsync(messageId);
            return NoContent();
        }

        // PUT: api/Messages/conversation/{conversationId}/destinataire/{destinataireId}/lus
        [HttpPut("conversation/{conversationId}/destinataire/{destinataireId}/lus")]
        public async Task<IActionResult> MarquerTousCommeLu(string conversationId, string destinataireId)
        {
            await _messageService.MarquerTousCommeLuAsync(conversationId, destinataireId);
            return NoContent();
        }
    }
}
