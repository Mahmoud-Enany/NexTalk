using Microsoft.AspNetCore.Identity;
using SignalRTask.Models;
using SignalRTask.Models.chat;

namespace SignalRTask.ViewModels.Chat
{
    public class PrivateChatVM
    {
        public IdentityUser Friend { get; set; } = null!;

        public List<PrivateMessage> Messages { get; set; } = new();

        public string NewMessage { get; set; } = string.Empty;

        public string FriendId { get; set; } = string.Empty;
    }
}