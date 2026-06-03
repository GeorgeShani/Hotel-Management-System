using System.Security.Claims;
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

        // The Identity id of the caller (JWT "sub" → ClaimTypes.NameIdentifier).
        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? string.Empty;

        // Admins and Managers may see and manage every reservation; Guests only their own.
        private bool IsPrivileged => User.IsInRole("Admin") || User.IsInRole("Manager");

        // GET: api/reservations
        [HttpGet]
        public async Task<IActionResult> GetAllReservations()
        {
            var reservations = await _reservationService.GetAllReservationsAsync(CurrentUserId, IsPrivileged);
            return Ok(reservations);
        }

        // GET: api/reservations/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservation(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id, CurrentUserId, IsPrivileged);
            if (reservation == null) return NotFound("Reservation not found.");

            return Ok(reservation);
        }

        // POST: api/reservations
        [HttpPost]
        [Authorize(Roles = "Guest")] // only a Guest can make a reservation
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto createDto)
        {
            // The service validates the dates and room availability and computes the price
            var reservation = await _reservationService.CreateReservationAsync(createDto, CurrentUserId);
            return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
        }

        // PUT: api/reservations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] UpdateReservationDto updateDto)
        {
            await _reservationService.UpdateReservationAsync(id, updateDto, CurrentUserId, IsPrivileged);
            return NoContent();
        }

        // DELETE: api/reservations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            await _reservationService.DeleteReservationAsync(id, CurrentUserId, IsPrivileged);
            return NoContent();
        }
    }
}
