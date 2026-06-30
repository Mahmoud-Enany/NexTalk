using Microsoft.AspNetCore.Identity;

namespace SignalRTask.Models
{
    public class PrivateMessage
    {
        public int Id { get; set; }

        public string SenderId { get; set; } = null!;

        public IdentityUser Sender { get; set; } = null!;

        public string ReceiverId { get; set; } = null!;

        public IdentityUser Receiver { get; set; } = null!;

        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; }

        public bool IsDelivered { get; set; }

        public bool IsSeen { get; set; }

        public bool IsDeleted { get; set; }
    }
}