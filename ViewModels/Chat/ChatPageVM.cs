using Microsoft.AspNetCore.Identity;
using SignalRTask.Models;
using SignalRTask.Models.chat;
using SignalRTask.Models.Friends;

namespace SignalRTask.ViewModels.Chat
{
    public class ChatPageVM
    {
        public List<Friend> Friends { get; set; } = new();

        public IdentityUser? SelectedFriend { get; set; }

        public string? SelectedFriendId { get; set; }

        public List<PrivateMessage> Messages { get; set; } = new();

        public string NewMessage { get; set; } = string.Empty;
    }
}