using BackEnd.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Data
{
    // Seeds roles, demo logins and sample data so the app is never empty (great for
    // demos). Rule: if the database is EMPTY, seed everything; if it already has any
    // data, do nothing. That single guard is what prevents duplicates, so it is safe
    // to run on every startup and to trigger again via POST /api/Seed.
    public static class DataSeeder
    {
        // Returns true if it seeded a fresh database, false if it did nothing (DB already had data).
        public static async Task<bool> SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<AppDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Only seed a fresh/empty database. If anything is already there, leave it alone.
            var isEmpty = !await context.Users.AnyAsync()
                       && !await context.Hotels.AnyAsync()
                       && !await context.Guests.AnyAsync()
                       && !await context.Reservations.AnyAsync();
            if (!isEmpty) return false;

            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);
            await SeedHotelsAndRoomsAsync(context);
            await SeedGuestsAsync(context);
            await SeedManagersAsync(context);
            await SeedReservationsAsync(context);
            return true;
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in new[] { "Admin", "Manager", "Guest" })
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
            // (email, password, firstName, lastName, role). The Guest logins match the
            // guest profiles below by email so those accounts can book / see their own.
            var demoUsers = new[]
            {
                ("admin@hms.local",   "Admin#123",   "Ada",  "Admin",   "Admin"),
                ("manager@hms.local", "Manager#123", "Max",  "Manager", "Manager"),
                ("john@hms.local",    "Guest#123",   "John", "Doe",     "Guest"),
                ("jane@hms.local",    "Guest#123",   "Jane", "Smith",   "Guest"),
            };

            foreach (var (email, password, firstName, lastName, role) in demoUsers)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, role);
            }
        }

        private static async Task SeedHotelsAndRoomsAsync(AppDbContext context)
        {
            var hotels = new (string Name, int Rating, string Country, string City, string Address, (string Name, decimal Price)[] Rooms)[]
            {
                ("Grand Plaza",    5, "Georgia", "Tbilisi",  "1 Rustaveli Ave",
                    new[] { ("101 - Deluxe", 180m), ("102 - Standard", 120m), ("201 - Suite", 320m), ("202 - Suite", 340m) }),
                ("Seaside Resort", 4, "Georgia", "Batumi",   "12 Beach Road",
                    new[] { ("A1 - Sea View", 150m), ("A2 - Standard", 100m), ("B1 - Suite", 260m) }),
                ("Mountain Lodge", 4, "Georgia", "Gudauri",  "5 Ski Slope",
                    new[] { ("Cabin 1", 140m), ("Cabin 2", 200m), ("Cabin 3", 220m) }),
                ("City Inn",       3, "Georgia", "Kutaisi",  "8 Central Sq",
                    new[] { ("11 - Single", 70m), ("12 - Double", 95m), ("13 - Double", 95m) }),
                ("Riverside Hotel",4, "Georgia", "Mtskheta", "3 River Walk",
                    new[] { ("R1 - River View", 160m), ("R2 - Standard", 110m), ("R3 - Suite", 240m) }),
                ("Desert Oasis",   5, "Georgia", "Telavi",   "20 Vineyard Rd",
                    new[] { ("O1 - Premium", 300m), ("O2 - Premium", 300m), ("O3 - Royal", 500m) }),
            };

            foreach (var h in hotels)
            {
                context.Hotels.Add(new Hotel
                {
                    Name = h.Name,
                    Rating = h.Rating,
                    Country = h.Country,
                    City = h.City,
                    Address = h.Address,
                    Rooms = h.Rooms.Select(r => new Room { Name = r.Name, Price = r.Price }).ToList()
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedGuestsAsync(AppDbContext context)
        {
            var guests = new (string First, string Last, string PersonalNumber, string Phone, string Email)[]
            {
                ("John",  "Doe",   "0000000001", "555000111", "john@hms.local"),
                ("Jane",  "Smith", "0000000003", "555000333", "jane@hms.local"),
                ("Bob",   "Brown", "0000000004", "555000444", "bob@hms.local"),
                ("Alice", "Green", "0000000005", "555000555", "alice@hms.local"),
                ("Tom",   "White", "0000000006", "555000666", "tom@hms.local"),
            };

            foreach (var g in guests)
            {
                context.Guests.Add(new Guest
                {
                    FirstName = g.First,
                    LastName = g.Last,
                    PersonalNumber = g.PersonalNumber,
                    PhoneNumber = g.Phone,
                    Email = g.Email
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedManagersAsync(AppDbContext context)
        {
            var managers = new (string First, string Last, string PersonalNumber, string Email, string Phone, string HotelName)[]
            {
                ("Max",  "Manager", "0000000002", "manager@hms.local", "555000222", "Grand Plaza"),
                ("Nina", "Novak",   "0000000007", "nina@hms.local",    "555000777", "Seaside Resort"),
                ("Leo",  "Lang",    "0000000008", "leo@hms.local",     "555000888", "Mountain Lodge"),
            };

            foreach (var m in managers)
            {
                var hotelId = await context.Hotels.Where(h => h.Name == m.HotelName)
                    .Select(h => h.Id).FirstOrDefaultAsync();
                if (hotelId == 0) continue;

                context.Managers.Add(new Manager
                {
                    FirstName = m.First,
                    LastName = m.Last,
                    PersonalNumber = m.PersonalNumber,
                    Email = m.Email,
                    PhoneNumber = m.Phone,
                    HotelId = hotelId
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedReservationsAsync(AppDbContext context)
        {
            var john = await context.Guests.FirstOrDefaultAsync(g => g.Email == "john@hms.local");
            var jane = await context.Guests.FirstOrDefaultAsync(g => g.Email == "jane@hms.local");
            var rooms = await context.Rooms.OrderBy(r => r.Id).Take(3).ToListAsync();
            if (rooms.Count < 2) return;

            if (john is not null)
                context.Reservations.Add(new Reservation
                {
                    GuestId = john.Id,
                    CheckInDate = DateTime.Today,
                    CheckOutDate = DateTime.Today.AddDays(3),
                    ReservationRooms = new List<ReservationRoom> { new() { RoomId = rooms[0].Id } }
                });

            if (jane is not null)
                context.Reservations.Add(new Reservation
                {
                    GuestId = jane.Id,
                    CheckInDate = DateTime.Today.AddDays(1),
                    CheckOutDate = DateTime.Today.AddDays(4),
                    ReservationRooms = new List<ReservationRoom> { new() { RoomId = rooms[1].Id } }
                });

            await context.SaveChangesAsync();
        }
    }
}
