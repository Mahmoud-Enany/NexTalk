using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignalRTask.Data;
using SignalRTask.Models.Friends;
using SignalRTask.ViewModels.Friends;
using System.Security.Claims;

namespace SignalRTask.Controllers
{
    [Authorize]
    public class FriendsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public FriendsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index(string? search)
        {
            FriendsIndexVM vm = new();

            vm.Search = search;

            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            List<string> friendsIds = await context.Friends
                .Where(f => f.UserId == currentUserId)
                .Select(f => f.FriendId)
                .ToListAsync();

            List<string> pendingRequests = await context.FriendRequests
                .Where(f =>
                    (f.SenderId == currentUserId || f.ReceiverId == currentUserId) &&
                    f.Status == FriendRequestStatus.Pending)
                .Select(f => f.SenderId == currentUserId
                    ? f.ReceiverId
                    : f.SenderId)
                .ToListAsync();

            vm.AvailableUsers = await userManager.Users
    .Where(u => u.Id != currentUserId)
    .Where(u => !friendsIds.Contains(u.Id))
    .Where(u => !pendingRequests.Contains(u.Id))
    .OrderBy(u => u.Email)
    .ToListAsync();

            vm.FriendRequests = await context.FriendRequests
                .Include(f => f.Sender)
                .Where(f =>
                    f.ReceiverId == currentUserId &&
                    f.Status == FriendRequestStatus.Pending)
                .OrderByDescending(f => f.RequestDate)
                .ToListAsync();

            vm.Friends = await context.Friends
                .Include(f => f.FriendUser)
                .Where(f => f.UserId == currentUserId).OrderBy(f => f.FriendUser.Email).ToListAsync();

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> SendRequest(string receiverId)
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            bool exists = await context.FriendRequests.AnyAsync(f =>
                ((f.SenderId == currentUserId && f.ReceiverId == receiverId) ||
                 (f.SenderId == receiverId && f.ReceiverId == currentUserId))
                 && f.Status == FriendRequestStatus.Pending);

            if (!exists)
            {
                FriendRequest request = new()
                {
                    SenderId = currentUserId,
                    ReceiverId = receiverId
                };

                context.FriendRequests.Add(request);

                await context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AcceptRequest(int requestId)
        {
            FriendRequest? request = await context.FriendRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                return RedirectToAction(nameof(Index));

            request.Status = FriendRequestStatus.Accepted;

            context.Friends.Add(new Friend
            {
                UserId = request.SenderId,
                FriendId = request.ReceiverId
            });

            context.Friends.Add(new Friend
            {
                UserId = request.ReceiverId,
                FriendId = request.SenderId
            });

            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Chat(string id)
        {
            return Content($"Chat with user : {id}");
        }
    }
}