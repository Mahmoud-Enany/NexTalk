namespace SignalRTask.Models;
using SignalRTask.Models.chat;
using SignalRTask.Models.Groups;

    public class Rooms
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public ICollection<RoomMember> Members { get; set; } = new List<RoomMember>();

    public ICollection<GroupMessage> Messages { get; set; } = new List<GroupMessage>();
}

