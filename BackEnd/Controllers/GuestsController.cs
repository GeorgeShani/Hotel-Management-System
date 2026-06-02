using BackEnd.DTOs.Guest;
using BackEnd.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // any logged-in role
    public class GuestsController : ControllerBase
    {
        private readonly IGuestService _guestService;

        public GuestsController(IGuestService guestService)
        {
            _guestService = guestService;
        }

        // GET: api/guests
        [HttpGet]
        public async Task<IActionResult> GetAllGuests()
        {
            var guests = await _guestService.GetAllGuestsAsync();
            return Ok(guests);
        }

        // GET: api/guests/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGuest(int id)
        {
            var guest = await _guestService.GetGuestByIdAsync(id);
            if (guest == null) return NotFound("Guest not found.");

            return Ok(guest);
        }

        // POST: api/guests
        [HttpPost]
        public async Task<IActionResult> CreateGuest([FromBody] CreateGuestDto createGuestDto)
        {
            var guest = await _guestService.CreateGuestAsync(createGuestDto);
            return CreatedAtAction(nameof(GetGuest), new { id = guest.Id }, guest);
        }

        // PUT: api/guests/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGuest(int id, [FromBody] UpdateGuestDto updateGuestDto)
        {
            await _guestService.UpdateGuestAsync(id, updateGuestDto);
            return NoContent();
        }

        // DELETE: api/guests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGuest(int id)
        {
            await _guestService.DeleteGuestAsync(id);
            return NoContent();
        }
    }
}
