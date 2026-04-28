using Microsoft.AspNetCore.Mvc;
using ConnectHub.Notification.Application.Interfaces;
using ConnectHub.Notification.Domain.Entities;

namespace ConnectHub.Notification.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> Get(string userId)
        {
            return Ok(await _service.GetUserNotifications(userId));
        }

        [HttpPost]
        public async Task<IActionResult> Create(global::ConnectHub.Notification.Domain.Entities.Notification notification)
        {
            await _service.CreateNotification(notification);
            return Ok("Notification created");
        }

        [HttpPut("read/{id}")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            await _service.MarkAsRead(id);
            return Ok("Marked as read");
        }
    }
}