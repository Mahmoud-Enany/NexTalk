using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignalRTask.Data;
using SignalRTask.Models;
using SignalRTask.ViewModels.Chat;
using System.Security.Claims;

namespace SignalRTask.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public ChatController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Private(string id)
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            ViewBag.CurrentUserId = currentUserId;

            var friend = await userManager.FindByIdAsync(id);

            if (friend == null)
                return NotFound();

            var messages = await context.PrivateMessages
    .Where(m =>
        (m.SenderId == currentUserId && m.ReceiverId == id)
        ||
        (m.SenderId == id && m.ReceiverId == currentUserId))
    .OrderBy(m => m.SentAt)
    .ToListAsync();

            PrivateChatVM vm = new()
            {
                Friend = friend,
                FriendId = id,
                Messages = messages
            };

            return View(vm);
        }
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var notifications = await context.Notifications
                .Where(n => n.UserId == currentUserId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToListAsync();

            return Json(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadNotificationsCount()
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            int count = await context.Notifications
                .CountAsync(n =>
                    n.UserId == currentUserId &&
                    !n.IsRead);

            return Json(count);
        }
        [HttpPost]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var notification = await context.Notifications
                .FirstOrDefaultAsync(n =>
                    n.Id == id &&
                    n.UserId == currentUserId);

            if (notification == null)
                return NotFound();

            notification.IsRead = true;

            await context.SaveChangesAsync();

            return Ok();
        }

    }
}