using BackEnd.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Data
{
    // We use IdentityDbContext for authentication and roles
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        public DbSet<ReservationRoom> ReservationRooms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Required for Identity tables to be generated correctly
            base.OnModelCreating(modelBuilder);

            // 1. Relationship: Hotel 1 -> M Manager
            modelBuilder.Entity<Manager>()
                .HasOne(m => m.Hotel)
                .WithMany(h => h.Managers)
                .HasForeignKey(m => m.HotelId)
                .OnDelete(DeleteBehavior.Restrict); // do not delete the manager automatically

            // 2. Relationship: Hotel 1 -> M Room
            modelBuilder.Entity<Room>()
                .HasOne(r => r.Hotel)
                .WithMany(h => h.Rooms)
                .HasForeignKey(r => r.HotelId)
                .OnDelete(DeleteBehavior.Restrict); // do not delete the room automatically

            // 3. Relationship: Guest 1 -> M Reservation
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Guest)
                .WithMany(g => g.Reservations)
                .HasForeignKey(r => r.GuestId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Relationship: Reservation M <-> M Room (Join Table)
            modelBuilder.Entity<ReservationRoom>()
                .HasKey(rr => new { rr.ReservationId, rr.RoomId });

            modelBuilder.Entity<ReservationRoom>()
                .HasOne(rr => rr.Reservation)
                .WithMany(r => r.ReservationRooms)
                .HasForeignKey(rr => rr.ReservationId);

            modelBuilder.Entity<ReservationRoom>()
                .HasOne(rr => rr.Room)
                .WithMany(r => r.ReservationRooms)
                .HasForeignKey(rr => rr.RoomId);

            // 5. Unique Constraints
            modelBuilder.Entity<Manager>().HasIndex(m => m.PersonalNumber).IsUnique();
            modelBuilder.Entity<Manager>().HasIndex(m => m.Email).IsUnique();

            modelBuilder.Entity<Guest>().HasIndex(g => g.PersonalNumber).IsUnique();
            modelBuilder.Entity<Guest>().HasIndex(g => g.PhoneNumber).IsUnique();

            // 6. Database-level validations
            modelBuilder.Entity<Room>()
                .Property(r => r.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Room>()
                .ToTable(t => t.HasCheckConstraint("CK_Room_Price", "Price > 0"));

            modelBuilder.Entity<Hotel>()
                .ToTable(t => t.HasCheckConstraint("CK_Hotel_Rating", "Rating >= 1 AND Rating <= 5"));
        }
    }
}
