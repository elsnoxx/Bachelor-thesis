using Microsoft.EntityFrameworkCore;
using server.Models;
using System;

namespace server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Player> Players { get; set; }
        public DbSet<BioData> BioDatas { get; set; }
        public DbSet<User> users { get; set; }
        }
}
