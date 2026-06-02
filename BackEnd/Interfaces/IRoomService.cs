using BackEnd.DTOs.Room;

namespace BackEnd.Interfaces
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomDto>> GetRoomsByHotelIdAsync(int hotelId);
        Task<RoomDto?> GetRoomByIdAsync(int id);
        Task<RoomDto> CreateRoomAsync(CreateRoomDto createRoomDto);
        Task UpdateRoomAsync(int id, UpdateRoomDto updateRoomDto);
        Task DeleteRoomAsync(int id);
    }
}
