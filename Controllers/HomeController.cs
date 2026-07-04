using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignalRTask.Data;
using SignalRTask.Models;
using SignalRTask.ViewModels;
using System.Diagnostics;

namespace SignalRTask.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public HomeController(ApplicationDbContext context,UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }
        public IActionResult Index()
        {
            string userId = userManager.GetUserId(User);

            HomeVM vm = new()
            {
                Rooms = context.Rooms.ToList(),

                Users = userManager.Users.ToList(),

                FriendsCount = context.Friends
                    .Count(x => x.UserId == userId),

                RoomsCount = context.Rooms.Count(),

                MessagesCount =
                    context.PrivateMessages.Count() +
                    context.GroupMessages.Count(),

                OnlineUsersCount = context.UserConnections
                    .Select(x => x.UserId)
                    .Distinct()
                    .Count()
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
