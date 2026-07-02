using Microsoft.AspNetCore.Identity;

namespace SignalRTask.Models.chat
{
    public class ActiveChat
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        public IdentityUser User { get; set; } = null!;

        public string FriendId { get; set; } = null!;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}