using SignalRTask.Models;
using SignalRTask.Models.chat;

namespace SignalRTask.ViewModels.Groups
{
    public class GroupChatVM
    {
        public Rooms Room { get; set; } = null!;

        public List<GroupMessage> Messages { get; set; } = new();

        public int MembersCount { get; set; }
        public List<string> OnlineUsers { get; set; } = new();
    }
}