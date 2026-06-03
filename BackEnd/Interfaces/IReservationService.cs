using BackEnd.DTOs.Reservation;

namespace BackEnd.Interfaces
{
    public interface IReservationService
    {
        // A privileged caller (Admin/Manager) sees and manages every reservation;
        // a Guest is restricted to the reservations they own (created).
        Task<IEnumerable<ReservationDto>> GetAllReservationsAsync(string userId, bool isPrivileged);
        Task<ReservationDto?> GetReservationByIdAsync(int id, string userId, bool isPrivileged);
        Task<ReservationDto> CreateReservationAsync(CreateReservationDto createDto, string userId);
        Task UpdateReservationAsync(int id, UpdateReservationDto updateDto, string userId, bool isPrivileged);
        Task DeleteReservationAsync(int id, string userId, bool isPrivileged);
    }
}
