using Microsoft.AspNetCore.Identity;
using SignalRTask.Models.Friends;

namespace SignalRTask.ViewModels.Friends
{
    public class FriendsIndexVM
    {
        public string? Search { get; set; }

        public List<IdentityUser> SearchResult { get; set; } = new();

        public List<FriendRequest> FriendRequests { get; set; } = new();

        public List<Friend> Friends { get; set; } = new();
        public List<IdentityUser> AvailableUsers { get; set; } = [];
    }
}