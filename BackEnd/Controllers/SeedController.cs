using BackEnd.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        // POST: api/seed
        // Re-runs the idempotent data seeder (roles, demo logins, sample data) on demand,
        // then returns the current row counts.
        // Open (no auth) on purpose: it's a demo/dev convenience. The seeder is idempotent
        // so repeated calls don't create duplicates. Lock this down before any real deployment.
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Seed()
        {
            // Use the request (scoped) service provider so scoped services like the
            // DbContext resolve correctly.
            var seeded = await DataSeeder.SeedAsync(HttpContext.RequestServices);

            var context = HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            return Ok(new
            {
                message = seeded
                    ? "Database was empty — seeded sample data."
                    : "Database already has data — nothing was seeded.",
                seeded,
                hotels = await context.Hotels.CountAsync(),
                rooms = await context.Rooms.CountAsync(),
                guests = await context.Guests.CountAsync(),
                managers = await context.Managers.CountAsync(),
                reservations = await context.Reservations.CountAsync(),
                users = await context.Users.CountAsync()
            });
        }
    }
}
