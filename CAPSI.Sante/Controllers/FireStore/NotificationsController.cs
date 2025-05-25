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
    public class NotificationsController : ControllerBase
    {
        private readonly FirestoreNotificationService _notificationService;

        public NotificationsController(FirestoreNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // POST: api/Notifications
        [HttpPost]
        public async Task<ActionResult<string>> EnvoyerNotification(Notification notification)
        {
            var id = await _notificationService.EnvoyerNotificationAsync(notification);
            return CreatedAtAction(nameof(GetNotifications), new { userId = notification.UserId }, id);
        }

        // GET: api/Notifications/{userId}/nonlues
        [HttpGet("{userId}/nonlues")]
        public async Task<ActionResult<List<Notification>>> GetNotificationsNonLues(string userId)
        {
            var notifications = await _notificationService.GetNotificationsNonLuesAsync(userId);
            return notifications;
        }

        // GET: api/Notifications/{userId}
        [HttpGet("{userId}")]
        public async Task<ActionResult<List<Notification>>> GetNotifications(string userId)
        {
            var notifications = await _notificationService.GetNotificationsAsync(userId);
            return notifications;
        }

        // PUT: api/Notifications/{notificationId}/lue
        [HttpPut("{notificationId}/lue")]
        public async Task<IActionResult> MarquerCommeLue(string notificationId)
        {
            await _notificationService.MarquerCommeLueAsync(notificationId);
            return NoContent();
        }

        // DELETE: api/Notifications/{notificationId}
        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> SupprimerNotification(string notificationId)
        {
            await _notificationService.SupprimerNotificationAsync(notificationId);
            return NoContent();
        }
    }
}
