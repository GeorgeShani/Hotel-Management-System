using BackEnd.DTOs.Room;
using BackEnd.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        // GET: api/rooms/hotel/1
        // (returns all rooms of a specific hotel)
        [HttpGet("hotel/{hotelId}")]
        public async Task<IActionResult> GetRoomsByHotel(int hotelId)
        {
            var rooms = await _roomService.GetRoomsByHotelIdAsync(hotelId);
            return Ok(rooms);
        }

        // GET: api/rooms/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoom(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null) return NotFound();

            return Ok(room);
        }

        // POST: api/rooms
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomDto createRoomDto)
        {
            // The service validates that the hotel exists and the price is correct
            var room = await _roomService.CreateRoomAsync(createRoomDto);
            return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
        }

        // PUT: api/rooms/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] UpdateRoomDto updateRoomDto)
        {
            await _roomService.UpdateRoomAsync(id, updateRoomDto);
            return NoContent();
        }

        // DELETE: api/rooms/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            await _roomService.DeleteRoomAsync(id);
            return NoContent();
        }
    }
}
