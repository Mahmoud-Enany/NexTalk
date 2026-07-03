using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignalRTask.Data;
using SignalRTask.Models;
using SignalRTask.ViewModels.Groups;
using System.Security.Claims;

namespace SignalRTask.Controllers
{
    [Authorize]
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext context;

        public GroupsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<IActionResult> Index()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var groups = await context.RoomMembers
                .Include(x => x.Room)
                .Where(x => x.UserId == userId)
                .Select(x => x.Room)
                .ToListAsync();

            return View(groups);
        }

        public async Task<IActionResult> Chat(int id)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            bool joined = await context.RoomMembers.AnyAsync(x =>
                x.RoomId == id &&
                x.UserId == userId);

            if (!joined)
                return RedirectToAction(nameof(Index));

            var room = await context.Rooms.Include(r => r.Messages).ThenInclude(m => m.Sender).Include(r => r.Members).FirstAsync(r => r.Id == id);

            foreach (var message in room.Messages)
            {
                if (!string.IsNullOrEmpty(message.Sender.UserName))
                {
                    message.Sender.UserName =
                        message.Sender.UserName.Split('@')[0];
                }
            }

            GroupChatVM vm = new()
            {
                Room = room,
                MembersCount = room.Members.Count,
                Messages = room.Messages
        .OrderBy(x => x.SentAt)
        .ToList()
            };

            return View(vm);
        }
    }
}