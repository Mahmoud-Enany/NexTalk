using Microsoft.AspNetCore.Identity;
using SignalRTask.Models;

namespace SignalRTask.ViewModels
{
    public class HomeVM
    {
        public List<Rooms> Rooms { get; set; } = new();
        public List<IdentityUser> Users { get; set; } = new();
    }
}
