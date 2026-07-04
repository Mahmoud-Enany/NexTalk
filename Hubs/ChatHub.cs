using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalRTask.Data;
using SignalRTask.Models;
using SignalRTask.Models.chat;
using SignalRTask.Models.Connection;
using SignalRTask.Models.Groups;

namespace SignalRTask.Hubs
{
    public class ChatHub: Hub
    {
        private static readonly Dictionary<string, string> ActiveChats = new();
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
            if (string.IsNullOrWhiteSpace(roomName))
                return;

            if (await context.Rooms.AnyAsync(r => r.Name == roomName))
                return;

            Rooms room = new()
            {
                Name = roomName.Trim()
            };

            context.Rooms.Add(room);

            await context.SaveChangesAsync();

            string user = Context.User?.Identity?.Name ?? "Desktop Client";

            await Clients.All.SendAsync(
                "RoomCreated",
                room.Id,
                room.Name,
                user);
        }

        public async Task JoinRoom(string roomName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

            string user = Context.User?.Identity?.Name ?? "Desktop Client";

            string userId = Context.UserIdentifier!;

            var room = await context.Rooms
                .FirstOrDefaultAsync(r => r.Name == roomName);

            if (room != null)
            {
                bool alreadyJoined = await context.RoomMembers.AnyAsync(x =>
                    x.RoomId == room.Id &&
                    x.UserId == userId);

                if (!alreadyJoined)
                {
                    context.RoomMembers.Add(new RoomMember
                    {
                        RoomId = room.Id,
                        UserId = userId
                    });

                    await context.SaveChangesAsync();
                }
            }

            await Clients.All.SendAsync(
                "UserJoinedRoom",
                user,
                roomName);
        }

        public async Task SendMessageToRoom(string roomName, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            string senderId = Context.UserIdentifier!;
            string senderName = (Context.User?.Identity?.Name ?? "Unknown").Split('@')[0];

            var room = await context.Rooms
                .FirstOrDefaultAsync(r => r.Name == roomName);

            if (room == null)
                return;

            GroupMessage groupMessage = new()
            {
                RoomId = room.Id,
                SenderId = senderId,
                Content = message.Trim()
            };

            context.GroupMessages.Add(groupMessage);

            await context.SaveChangesAsync();

            await Clients.Group(roomName)
    .SendAsync(
        "ReceiveRoomMessage",
        groupMessage.Id,
        senderId,
        senderName,
        groupMessage.Content,
        groupMessage.SentAt);
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
                Content = message,
                IsDelivered = false,
                IsSeen = false,
                IsRead = false
            };

            context.PrivateMessages.Add(privateMessage);

            await context.SaveChangesAsync();

            bool receiverViewingThisChat = false;

            lock (ActiveChats)
            {
                receiverViewingThisChat =
                    ActiveChats.TryGetValue(receiverId, out var openedChat)
                    && openedChat == senderId;
            }

            if (!receiverViewingThisChat)
            {
                Notification notification = new()
                {
                    UserId = receiverId,
                    Title = "New Message",
                    Content = $"{senderName}: {message}",
                    Type = NotificationType.PrivateMessage,
                    Url = $"/Chat/Private/{senderId}"
                };

                context.Notifications.Add(notification);

                await context.SaveChangesAsync();

                await Clients.User(receiverId)
                    .SendAsync(
                        "ReceiveNotification",
                        notification.Id,
                        notification.Title,
                        notification.Content,
                        notification.Url);
            }

            await Clients.User(receiverId)
                .SendAsync(
                    "ReceivePrivateChatMessage",
                    privateMessage.Id,
                    senderId,
                    senderName,
                    message,
                    privateMessage.SentAt);

            privateMessage.IsDelivered = true;

            await context.SaveChangesAsync();

            await Clients.Caller.SendAsync(
                "MessageDelivered",
                privateMessage.Id);

            await Clients.Caller.SendAsync(
                "ReceivePrivateChatMessage",
                privateMessage.Id,
                senderId,
                senderName,
                message,
                privateMessage.SentAt);
        }
        public async Task MarkMessageDelivered(int messageId)
        {
            var message = await context.PrivateMessages.FindAsync(messageId);

            if (message == null)
                return;

            if (!message.IsDelivered)
            {
                message.IsDelivered = true;

                await context.SaveChangesAsync();
            }

            await Clients.User(message.SenderId)
                .SendAsync("MessageDelivered", messageId);
        }
        public async Task MarkMessageSeen(int messageId)
        {
            var message = await context.PrivateMessages.FindAsync(messageId);

            if (message == null)
                return;

            if (!message.IsSeen)
            {
                message.IsSeen = true;

                message.IsRead = true;

                await context.SaveChangesAsync();
            }

            await Clients.User(message.SenderId)
                .SendAsync("MessageSeen", messageId);
        }

        public async Task Typing(string receiverId)
        {
            string senderName = Context.User?.Identity?.Name ?? "Unknown";

            await Clients.User(receiverId)
                .SendAsync("UserTyping", senderName);
        }
        public override async Task OnConnectedAsync()
        {
            if (Context.UserIdentifier != null)
            {
                var oldConnections = context.UserConnections
                    .Where(x => x.UserId == Context.UserIdentifier);

                context.UserConnections.RemoveRange(oldConnections);

                context.UserConnections.Add(new UserConnection
                {
                    UserId = Context.UserIdentifier,
                    ConnectionId = Context.ConnectionId
                });

                await context.SaveChangesAsync();

                await Clients.All.SendAsync("UserOnline", Context.UserIdentifier);
                await Clients.All.SendAsync("GroupOnlineUsersChanged");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connection = await context.UserConnections
                .FirstOrDefaultAsync(x => x.ConnectionId == Context.ConnectionId);

            if (connection != null)
            {
                context.UserConnections.Remove(connection);

                await context.SaveChangesAsync();

                bool stillOnline = await context.UserConnections
                    .AnyAsync(x => x.UserId == connection.UserId);

                if (!stillOnline)
                {
                    await Clients.All.SendAsync("UserOffline", connection.UserId);
                    await Clients.All.SendAsync("GroupOnlineUsersChanged");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
        public async Task<bool> IsUserOnline(string userId)
        {
            return await context.UserConnections
                .AnyAsync(x => x.UserId == userId);
        }
        public Task SetActiveChat(string friendId)
        {
            string userId = Context.UserIdentifier!;

            lock (ActiveChats)
            {
                ActiveChats[userId] = friendId;
            }

            return Task.CompletedTask;
        }

        public Task ClearActiveChat()
        {
            string userId = Context.UserIdentifier!;

            lock (ActiveChats)
            {
                ActiveChats.Remove(userId);
            }

            return Task.CompletedTask;
        }

        public async Task DeleteMessage(int messageId)
        {
            var message = await context.PrivateMessages.FindAsync(messageId);

            if (message == null || message.IsDeleted)
                return;

            message.IsDeleted = true;

            await context.SaveChangesAsync();

            await Clients.Users(message.SenderId, message.ReceiverId)
                .SendAsync("MessageDeleted", messageId);
        }

        public async Task EditMessage(int messageId, string newMessage)
        {
            var message = await context.PrivateMessages.FindAsync(messageId);

            if (message == null || message.IsDeleted)
                return;

            if (string.IsNullOrWhiteSpace(newMessage))
                return;

            message.Content = newMessage.Trim();

            await context.SaveChangesAsync();

            await Clients.Users(message.SenderId, message.ReceiverId)
                .SendAsync(
                    "MessageEdited",
                    messageId,
                    message.Content);
        }

        public async Task DeleteGroupMessage(int messageId)
        {
            string userId = Context.UserIdentifier!;

            var message = await context.GroupMessages
                .Include(x => x.Room)
                .FirstOrDefaultAsync(x => x.Id == messageId);

            if (message == null)
                return;

            if (message.SenderId != userId)
                return;

            if (message.IsDeleted)
                return;

            message.IsDeleted = true;

            await context.SaveChangesAsync();

            await Clients.Group(message.Room.Name)
                .SendAsync("GroupMessageDeleted", messageId);
        }

        public async Task EditGroupMessage(int messageId, string newContent)
        {
            string userId = Context.UserIdentifier!;

            var message = await context.GroupMessages
                .Include(x => x.Room)
                .FirstOrDefaultAsync(x => x.Id == messageId);

            if (message == null)
                return;

            if (message.SenderId != userId)
                return;

            if (message.IsDeleted)
                return;

            if (string.IsNullOrWhiteSpace(newContent))
                return;

            message.Content = newContent.Trim();
            message.IsEdited = true;

            await context.SaveChangesAsync();

            await Clients.Group(message.Room.Name)
                .SendAsync(
                    "GroupMessageEdited",
                    message.Id,
                    message.Content);
        }



    }

}
