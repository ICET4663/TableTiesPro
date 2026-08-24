using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using TableTies.Models;

namespace TableTies.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Organization> Organizations { get; set; } = default!;
        public DbSet<Restaurant> Restaurants { get; set; } = default!;
        public DbSet<RestaurantTable> RestaurantTables { get; set; } = default!;
        public DbSet<TableBooking> TableBookings { get; set; } = default!;
        public DbSet<Hotel> Hotels { get; set; } = default!;
        public DbSet<RoomBooking> RoomBookings { get; set; } = default!;
        public DbSet<Booking> Bookings { get; set; } = default!;

        public DbSet<Consultant> Consultants { get; set; } = default!;
        public DbSet<ConsultantBooking> ConsultantBookings { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole<Guid>>(entity =>
            {
                entity.ToTable("AspNetRoles");
                entity.Property(r => r.ConcurrencyStamp).HasColumnType("TEXT");
                entity.Property(r => r.Name).HasMaxLength(256);
                entity.Property(r => r.NormalizedName).HasMaxLength(256);
            });

            builder.Entity<ApplicationUser>(entity =>
            {
                 entity.ToTable("AspNetUsers");
                entity.Property(u => u.ConcurrencyStamp).HasColumnType("TEXT");
                entity.Property(u => u.SecurityStamp).HasColumnType("TEXT");
                entity.Property(u => u.UserName).HasMaxLength(256);
                entity.Property(u => u.NormalizedUserName).HasMaxLength(256);
                entity.Property(u => u.Email).HasMaxLength(256);
                entity.Property(u => u.NormalizedEmail).HasMaxLength(256);
            });

             builder.Entity<IdentityUserClaim<Guid>>(entity =>
             {
                 entity.ToTable("AspNetUserClaims");
                 entity.Property(uc => uc.ClaimType).HasColumnType("TEXT");
                 entity.Property(uc => uc.ClaimValue).HasColumnType("TEXT");
             });

             builder.Entity<IdentityUserLogin<Guid>>(entity =>
             {
                 entity.ToTable("AspNetUserLogins");
                 entity.Property(ul => ul.ProviderKey).HasColumnType("TEXT").HasMaxLength(128);
                 entity.Property(ul => ul.ProviderDisplayName).HasColumnType("TEXT");
                 entity.Property(ul => ul.LoginProvider).HasMaxLength(128);
             });

             builder.Entity<IdentityUserToken<Guid>>(entity =>
             {
                 entity.ToTable("AspNetUserTokens");
                 entity.Property(ut => ut.LoginProvider).HasMaxLength(128);
                 entity.Property(ut => ut.Name).HasMaxLength(128);
                 entity.Property(ut => ut.Value).HasColumnType("TEXT");
             });

              builder.Entity<IdentityRoleClaim<Guid>>(entity =>
              {
                  entity.ToTable("AspNetRoleClaims");
                  entity.Property(rc => rc.ClaimType).HasColumnType("TEXT");
                  entity.Property(rc => rc.ClaimValue).HasColumnType("TEXT");
              });

               builder.Entity<IdentityUserRole<Guid>>(entity =>
               {
                   entity.ToTable("AspNetUserRoles");
               });


            builder.Entity<Organization>()
                .HasMany(o => o.Restaurants)
                .WithOne(r => r.Organization)
                .HasForeignKey(r => r.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Restaurant>()
                .HasMany(r => r.RestaurantTables)
                .WithOne(t => t.Restaurant)
                .HasForeignKey(t => t.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Restaurant>()
                .HasMany(r => r.TableBookings)
                .WithOne(tb => tb.Restaurant)
                .HasForeignKey(tb => tb.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RestaurantTable>()
                .HasMany(t => t.TableBookings)
                .WithOne(tb => tb.Table)
                .HasForeignKey(tb => tb.TableId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.TableBookings)
                .WithOne(tb => tb.User)
                .HasForeignKey(tb => tb.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Hotel>()
                .HasMany(h => h.RoomBookings)
                .WithOne(rb => rb.Hotel)
                .HasForeignKey(rb => rb.HotelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.RoomBookings)
                .WithOne(rb => rb.User)
                .HasForeignKey(rb => rb.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.Bookings)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Booking>()
                .HasOne(b => b.Restaurant)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.RestaurantId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.Table)
                .WithMany()
                .HasForeignKey(b => b.TableId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Consultant>()
                .HasMany(c => c.ConsultantBookings)
                .WithOne(cb => cb.Consultant)
                .HasForeignKey(cb => cb.ConsultantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.ConsultantBookings)
                .WithOne(cb => cb.User)
                .HasForeignKey(cb => cb.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            SeedData(builder);
        }

        private void SeedData(ModelBuilder builder)
        {
            var org1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var org2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");

            var rest1Id = Guid.Parse("aaaaaaa1-0000-0000-0000-000000000001");
            var rest2Id = Guid.Parse("aaaaaaa2-0000-0000-0000-000000000002");
            var rest3Id = Guid.Parse("aaaaaaa3-0000-0000-0000-000000000003");

            var table1Id = Guid.Parse("bbbbbbb1-0000-0000-0000-000000000001");
            var table2Id = Guid.Parse("bbbbbbb2-0000-0000-0000-000000000002");
            var table3Id = Guid.Parse("bbbbbbb3-0000-0000-0000-000000000003");
            var table4Id = Guid.Parse("bbbbbbb4-0000-0000-0000-000000000004");
            var table5Id = Guid.Parse("bbbbbbb5-0000-0000-0000-000000000005");

            // Seed Consultant Data (using fixed GUIDs)
            builder.Entity<Consultant>().HasData(
                new Consultant { Id = Guid.Parse("cccccccc-0000-0000-0000-000000000001"), Name = "Alice Smith", Specialty = "Business Strategy" },
                new Consultant { Id = Guid.Parse("cccccccc-0000-0000-0000-000000000002"), Name = "Bob Johnson", Specialty = "Technical Consulting" },
                new Consultant { Id = Guid.Parse("cccccccc-0000-0000-0000-000000000003"), Name = "Charlie Brown", Specialty = "Marketing" }
            );
        }
    }
}
