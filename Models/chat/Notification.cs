using Microsoft.AspNetCore.Identity;

namespace SignalRTask.Models.chat
{
    public enum NotificationType
    {
        FriendRequest,
        FriendAccepted,

        PrivateMessage,
        GroupMessage,

        UserJoinedGroup,
        UserLeftGroup,

        RoomCreated,

        General
    }

    public class Notification
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        public IdentityUser User { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public NotificationType Type { get; set; }

        public string? Url { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}