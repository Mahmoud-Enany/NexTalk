using Microsoft.AspNetCore.Identity;
using SignalRTask.Models;

namespace SignalRTask.ViewModels
{
    public class HomeVM
    {
        public List<Rooms> Rooms { get; set; } = new();
        public List<IdentityUser> Users { get; set; } = new();
        public int FriendsCount { get; set; }

        public int RoomsCount { get; set; }

        public int MessagesCount { get; set; }

        public int OnlineUsersCount { get; set; }
    }
}
