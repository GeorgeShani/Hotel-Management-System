using BackEnd.DTOs.Hotel;
using BackEnd.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    // Base route: api/hotels
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private readonly IHotelService _hotelService;

        public HotelsController(IHotelService hotelService)
        {
            _hotelService = hotelService;
        }

        // GET: api/hotels?country=Geo&city=Tbilisi&rating=5
        [HttpGet]
        public async Task<IActionResult> GetHotels([FromQuery] string? country, [FromQuery] string? city, [FromQuery] int? rating)
        {
            var hotels = await _hotelService.GetHotelsAsync(country, city, rating);
            return Ok(hotels);
        }

        // GET: api/hotels/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetHotel(int id)
        {
            var hotel = await _hotelService.GetHotelByIdAsync(id);
            if (hotel == null) return NotFound();

            return Ok(hotel);
        }

        // POST: api/hotels
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateHotel([FromBody] CreateHotelDto createHotelDto)
        {
            var hotel = await _hotelService.CreateHotelAsync(createHotelDto);
            // Returns 201 Created and the link to the created hotel
            return CreatedAtAction(nameof(GetHotel), new { id = hotel.Id }, hotel);
        }

        // PUT: api/hotels/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateHotel(int id, [FromBody] UpdateHotelDto updateHotelDto)
        {
            await _hotelService.UpdateHotelAsync(id, updateHotelDto);
            return NoContent(); // 204 No Content on successful update
        }

        // DELETE: api/hotels/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            await _hotelService.DeleteHotelAsync(id);
            return NoContent(); // 204 No Content on successful delete
        }
    }
}
