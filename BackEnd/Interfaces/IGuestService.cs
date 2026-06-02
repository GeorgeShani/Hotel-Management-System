using BackEnd.DTOs.Guest;

namespace BackEnd.Interfaces
{
    public interface IGuestService
    {
        Task<IEnumerable<GuestDto>> GetAllGuestsAsync();
        Task<GuestDto?> GetGuestByIdAsync(int id);
        Task<GuestDto> CreateGuestAsync(CreateGuestDto createGuestDto);
        Task UpdateGuestAsync(int id, UpdateGuestDto updateGuestDto);
        Task DeleteGuestAsync(int id);
    }
}
