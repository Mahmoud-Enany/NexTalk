using Microsoft.AspNetCore.Identity;

namespace SignalRTask.Models.Friends
{
    public enum FriendRequestStatus
    {
        Pending,
        Accepted,
        Rejected
    }

    public class FriendRequest
    {
        public int Id { get; set; }

        public string SenderId { get; set; } = null!;

        public IdentityUser Sender { get; set; } = null!;

        public string ReceiverId { get; set; } = null!;

        public IdentityUser Receiver { get; set; } = null!;

        public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    }
}