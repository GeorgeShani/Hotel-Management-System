using BackEnd.DTOs.Reservation;

namespace BackEnd.Interfaces
{
    public interface IReservationService
    {
        // A privileged caller (Admin/Manager) sees and manages every reservation;
        // a Guest is restricted to reservations whose guest record matches their
        // login email (userEmail).
        Task<IEnumerable<ReservationDto>> GetAllReservationsAsync(string userEmail, bool isPrivileged);
        Task<ReservationDto?> GetReservationByIdAsync(int id, string userEmail, bool isPrivileged);
        Task<ReservationDto> CreateReservationAsync(CreateReservationDto createDto, string userEmail, bool isPrivileged);
        Task UpdateReservationAsync(int id, UpdateReservationDto updateDto, string userEmail, bool isPrivileged);
        Task DeleteReservationAsync(int id, string userEmail, bool isPrivileged);
    }
}
