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

            var roomIds = await context.RoomMembers
                .Where(x => x.UserId == userId)
                .Select(x => x.RoomId)
                .ToListAsync();

            var groups = await context.Rooms
                .Where(r => roomIds.Contains(r.Id))
                .Include(r => r.Members)
                .Include(r => r.Messages)
                    .ThenInclude(m => m.Sender)
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

            var room = await context.Rooms.Include(r => r.Messages).ThenInclude(m => m.Sender).Include(r => r.Members).ThenInclude(x => x.User).FirstAsync(r => r.Id == id);

            foreach (var message in room.Messages)
            {
                if (!string.IsNullOrEmpty(message.Sender.UserName))
                {
                    message.Sender.UserName =
                        message.Sender.UserName.Split('@')[0];
                }
            }

            var onlineIds = await context.UserConnections.Select(x => x.UserId).Distinct().ToListAsync();

            GroupChatVM vm = new()
            {
                Room = room,

                MembersCount = room.Members.Count,

                OnlineUsers = room.Members
                    .Where(x => onlineIds.Contains(x.UserId))
                    .Select(x => x.User.UserName!.Split('@')[0])
                    .ToList(),

                Messages = room.Messages
                    .OrderBy(x => x.SentAt)
                    .ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetOnlineMembers(int roomId)
        {
            var onlineIds = await context.UserConnections
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync();

            var users = await context.RoomMembers
                .Include(x => x.User)
                .Where(x => x.RoomId == roomId &&
                            onlineIds.Contains(x.UserId))
                .Select(x => x.User.UserName!.Split('@')[0])
                .ToListAsync();

            return Json(users);
        }


    }
}