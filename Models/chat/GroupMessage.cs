using Microsoft.AspNetCore.Identity;

namespace SignalRTask.Models.chat
{
    public class GroupMessage
    {
        public int Id { get; set; }

        public int RoomId { get; set; }

        public Rooms Room { get; set; } = null!;

        public string SenderId { get; set; } = null!;

        public IdentityUser Sender { get; set; } = null!;

        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }

        public bool IsEdited { get; set; }
    }
}