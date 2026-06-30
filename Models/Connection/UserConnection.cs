using Microsoft.AspNetCore.Identity;

namespace SignalRTask.Models.Connection
{
    public class UserConnection
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        public IdentityUser User { get; set; } = null!;

        public string ConnectionId { get; set; } = null!;

        public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    }
}