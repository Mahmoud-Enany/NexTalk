using Microsoft.AspNetCore.SignalR;
using SignalRTask.Data;
using SignalRTask.Models;

namespace SignalRTask.Hubs
{
    public class ChatHub: Hub
    {
        private readonly ApplicationDbContext context;

        public ChatHub(ApplicationDbContext context)
        {
            this.context = context;
        }
        public async Task SendMessage(string message)
        {
            string user = Context.User.Identity.Name;
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task CreateRoom(string roomName)
        {
            Rooms room = new Rooms();
            room.Name = roomName;
            context.Rooms.Add(room);
            context.SaveChanges();
            //string user = Context.User.Identity.Name;
            string user = Context.User?.Identity?.Name ?? "Desktop Client";
            await Clients.All.SendAsync("RoomCreated", room.Id, room.Name, user);
        }

        public async Task JoinRoom(string roomName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

            //string user = Context.User.Identity.Name;
            string user = Context.User?.Identity?.Name ?? "Desktop Client";
            await Clients.All.SendAsync("UserJoinedRoom", user, roomName);
        }

        public async Task SendMessageToRoom(string roomName, string message)
        {
            //string user = Context.User.Identity.Name;
            string user = Context.User?.Identity?.Name ?? "Desktop Client";

            await Clients.Group(roomName)
                         .SendAsync("ReceiveRoomMessage", user, message);
        }

        public async Task SendPrivateMessage(string userId, string message)
        {
            //string sender = Context.User.Identity.Name;
            string sender = Context.User?.Identity?.Name ?? "Desktop Client";
            await Clients.User(userId)
                         .SendAsync("ReceivePrivateMessage", sender, message);
        }

        public async Task SendDesktopMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public List<string> GetAllRooms()
        {
            return context.Rooms
                          .Select(r => r.Name)
                          .ToList();
        }

        public List<UserDTO> GetAllUsers()
        {
            return context.Users
                          .Select(u => new UserDTO
                          {
                              Id = u.Id,
                              Email = u.Email
                          })
                          .ToList();
        }
        public async Task SendPrivateChatMessage(string receiverId, string message)
        {
            string senderId = Context.UserIdentifier!;

            string senderName = Context.User?.Identity?.Name ?? "Unknown";

            PrivateMessage privateMessage = new()
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = message
            };

            context.PrivateMessages.Add(privateMessage);

            await context.SaveChangesAsync();

            await Clients.User(receiverId)
                .SendAsync(
                    "ReceivePrivateChatMessage",
                    senderId,
                    senderName,
                    message,
                    privateMessage.SentAt);

            await Clients.Caller
                .SendAsync(
                    "ReceivePrivateChatMessage",
                    senderId,
                    senderName,
                    message,
                    privateMessage.SentAt);
        }


    }

}
