using System.IdentityModel.Tokens.Jwt;
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

        // The email the caller signed in with (JWT "email" claim). A guest's
        // reservations are matched to their Guest record by this email.
        private string CurrentUserEmail =>
            User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? User.FindFirstValue("email")
            ?? string.Empty;

        // Admins and Managers may see and manage every reservation; Guests only their own.
        private bool IsPrivileged => User.IsInRole("Admin") || User.IsInRole("Manager");

        // GET: api/reservations
        [HttpGet]
        public async Task<IActionResult> GetAllReservations()
        {
            var reservations = await _reservationService.GetAllReservationsAsync(CurrentUserEmail, IsPrivileged);
            return Ok(reservations);
        }

        // GET: api/reservations/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservation(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id, CurrentUserEmail, IsPrivileged);
            if (reservation == null) return NotFound("Reservation not found.");

            return Ok(reservation);
        }

        // POST: api/reservations
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Guest")] // any signed-in role can make a reservation
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto createDto)
        {
            // The service validates the dates and room availability and computes the price
            var reservation = await _reservationService.CreateReservationAsync(createDto, CurrentUserEmail, IsPrivileged);
            return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
        }

        // PUT: api/reservations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] UpdateReservationDto updateDto)
        {
            await _reservationService.UpdateReservationAsync(id, updateDto, CurrentUserEmail, IsPrivileged);
            return NoContent();
        }

        // DELETE: api/reservations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            await _reservationService.DeleteReservationAsync(id, CurrentUserEmail, IsPrivileged);
            return NoContent();
        }
    }
}
