using BackEnd.DTOs.Reservation;
using BackEnd.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        // GET: api/reservations
        [HttpGet]
        public async Task<IActionResult> GetAllReservations()
        {
            var reservations = await _reservationService.GetAllReservationsAsync();
            return Ok(reservations);
        }

        // GET: api/reservations/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservation(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound("Reservation not found.");

            return Ok(reservation);
        }

        // POST: api/reservations
        [HttpPost]
        [Authorize(Roles = "Guest")] // only a Guest can make a reservation
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto createDto)
        {
            // The service validates the dates and room availability and computes the price
            var reservation = await _reservationService.CreateReservationAsync(createDto);
            return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
        }

        // PUT: api/reservations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] UpdateReservationDto updateDto)
        {
            await _reservationService.UpdateReservationAsync(id, updateDto);
            return NoContent();
        }

        // DELETE: api/reservations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            await _reservationService.DeleteReservationAsync(id);
            return NoContent();
        }
    }
}
