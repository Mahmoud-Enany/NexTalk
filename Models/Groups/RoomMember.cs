using Microsoft.AspNetCore.Identity;

namespace SignalRTask.Models.Groups
{
    public class RoomMember
    {
        public int Id { get; set; }

        public int RoomId { get; set; }

        public Rooms Room { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public IdentityUser User { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}