using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Models.DB;
using server.Repositories;
using System;

namespace server.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<GameRoom> GameRooms => Set<GameRoom>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<BioFeedback> BioFeedbacks => Set<BioFeedback>();
        public DbSet<Statistic> Statistics => Set<Statistic>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relace: User → GameRoom
            modelBuilder.Entity<GameRoom>()
                .HasOne(g => g.Creator)
                .WithMany(u => u.CreatedRooms)
                .HasForeignKey(g => g.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Relace: User → Session
            modelBuilder.Entity<Session>()
                .HasOne(s => s.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.UserId);

            // Relace: User -> RefreshToken
            modelBuilder.Entity<RefreshToken>()
                .HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId);

            // Relace: GameRoom → Session
            modelBuilder.Entity<Session>()
                .HasOne(s => s.GameRoom)
                .WithMany(g => g.Sessions)
                .HasForeignKey(s => s.GameRoomId);

            // Relace: Session → BioFeedback
            modelBuilder.Entity<BioFeedback>()
                .HasOne(b => b.Session)
                .WithMany(s => s.BioFeedbacks)
                .HasForeignKey(b => b.SessionId);

            // Relace: User → Statistic
            modelBuilder.Entity<Statistic>()
                .HasOne(s => s.User)
                .WithMany(u => u.Statistics)
                .HasForeignKey(s => s.UserId);

            // Relace: User → RefreshToken
            modelBuilder.Entity<RefreshToken>()
                .HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId);

            // indexes
            modelBuilder.Entity<Session>()
                .HasIndex(s => new { s.UserId, s.IsActive })
                .HasDatabaseName("idx_session_user_active");

            modelBuilder.Entity<BioFeedback>()
                .HasIndex(b => new { b.SessionId, b.Timestamp })
                .HasDatabaseName("idx_biofeedback_session_time");

            modelBuilder.Entity<GameRoom>()
                .HasIndex(g => new { g.Status, g.GameType })
                .HasDatabaseName("idx_gameroom_status_type");

            modelBuilder.Entity<Statistic>()
                .HasIndex(st => new { st.UserId, st.GameType })
                .HasDatabaseName("idx_statistic_user_game");
        }
    }
}
