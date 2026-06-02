using BackEnd.DTOs.Hotel;

namespace BackEnd.Interfaces
{
    public interface IHotelService
    {
        Task<IEnumerable<HotelDto>> GetHotelsAsync(string? country, string? city, int? rating);
        Task<HotelDto?> GetHotelByIdAsync(int id);
        Task<HotelDto> CreateHotelAsync(CreateHotelDto createHotelDto);
        Task UpdateHotelAsync(int id, UpdateHotelDto updateHotelDto);
        Task DeleteHotelAsync(int id);
    }
}
