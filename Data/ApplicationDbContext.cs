using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SignalRTask.Models;
using SignalRTask.Models.chat;
using SignalRTask.Models.Connection;
using SignalRTask.Models.Friends;
using SignalRTask.Models.Groups;

namespace SignalRTask.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext(options)
    {
        public DbSet<Rooms> Rooms { get; set; }

        public DbSet<PrivateMessage> PrivateMessages { get; set; }

        public DbSet<GroupMessage> GroupMessages { get; set; }

        public DbSet<Friend> Friends { get; set; }

        public DbSet<FriendRequest> FriendRequests { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<RoomMember> RoomMembers { get; set; }

        public DbSet<UserConnection> UserConnections { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<FriendRequest>()
                .HasOne(f => f.Sender)
                .WithMany()
                .HasForeignKey(f => f.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FriendRequest>()
                .HasOne(f => f.Receiver)
                .WithMany()
                .HasForeignKey(f => f.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Friend>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Friend>()
                .HasOne(f => f.FriendUser)
                .WithMany()
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PrivateMessage>()
                .HasOne(p => p.Sender)
                .WithMany()
                .HasForeignKey(p => p.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PrivateMessage>()
                .HasOne(p => p.Receiver)
                .WithMany()
                .HasForeignKey(p => p.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<GroupMessage>()
                .HasOne(g => g.Sender)
                .WithMany()
                .HasForeignKey(g => g.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<GroupMessage>()
                .HasOne(g => g.Room)
                .WithMany(r => r.Messages)
                .HasForeignKey(g => g.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RoomMember>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RoomMember>()
                .HasOne(r => r.Room)
                .WithMany(r => r.Members)
                .HasForeignKey(r => r.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserConnection>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Friend>()
                .HasIndex(f => new { f.UserId, f.FriendId })
                .IsUnique();

            builder.Entity<FriendRequest>()
                .HasIndex(f => new { f.SenderId, f.ReceiverId })
                .IsUnique();

            builder.Entity<RoomMember>()
                .HasIndex(r => new { r.RoomId, r.UserId })
                .IsUnique();

            builder.Entity<UserConnection>()
                .HasIndex(c => c.ConnectionId)
                .IsUnique();
        }
    }
}