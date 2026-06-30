using Microsoft.AspNetCore.Identity;

namespace SignalRTask.Models.Friends
{
    public class Friend
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        public IdentityUser User { get; set; } = null!;

        public string FriendId { get; set; } = null!;

        public IdentityUser FriendUser { get; set; } = null!;

        public DateTime FriendsSince { get; set; } = DateTime.UtcNow;
    }
}