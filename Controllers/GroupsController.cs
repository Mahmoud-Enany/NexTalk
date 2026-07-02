using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignalRTask.Data;
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
    }
}