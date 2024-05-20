using Kordalski_Projekt.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Kordalski_Projekt.Data
{
        public class ApplicationDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Series>()
                .HasMany(s => s.Episode)
                .WithOne(e => e.Series)
                .OnDelete(DeleteBehavior.Cascade);
        }

            public DbSet<Comments> Comments { get; set; }
            public DbSet<Episode> Episode { get; set; }
            public DbSet<Likes> Likes { get; set; }
            public DbSet<Ratings> Ratings { get; set; }
            public DbSet<Series> Series { get; set; }
            public DbSet<WatchedList> WatchedList { get; set; }
        }
    }
