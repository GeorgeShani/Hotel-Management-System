using AutoMapper;
using BackEnd.DTOs.Room;
using BackEnd.Interfaces;
using BackEnd.Models;

namespace BackEnd.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IHotelRepository _hotelRepository; // needed to validate the hotel
        private readonly IMapper _mapper;

        public RoomService(IRoomRepository roomRepository, IHotelRepository hotelRepository, IMapper mapper)
        {
            _roomRepository = roomRepository;
            _hotelRepository = hotelRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RoomDto>> GetRoomsByHotelIdAsync(int hotelId)
        {
            var rooms = await _roomRepository.GetRoomsByHotelIdAsync(hotelId);
            return _mapper.Map<IEnumerable<RoomDto>>(rooms);
        }

        public async Task<RoomDto?> GetRoomByIdAsync(int id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            return room == null ? null : _mapper.Map<RoomDto>(room);
        }

        public async Task<RoomDto> CreateRoomAsync(CreateRoomDto createRoomDto)
        {
            // 1. Check whether such a hotel exists in the database
            var hotelExists = await _hotelRepository.GetByIdAsync(createRoomDto.HotelId);
            if (hotelExists == null)
                throw new Exception("Hotel with the specified ID not found.");

            // 2. Validate the price (the CheckConstraint also guards this at the DB level)
            if (createRoomDto.Price <= 0)
                throw new ArgumentException("The room price must be greater than 0.");

            var roomEntity = _mapper.Map<Room>(createRoomDto);
            var createdRoom = await _roomRepository.AddAsync(roomEntity);

            return _mapper.Map<RoomDto>(createdRoom);
        }

        public async Task UpdateRoomAsync(int id, UpdateRoomDto updateRoomDto)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null)
                throw new Exception("Room not found.");

            if (updateRoomDto.Price <= 0)
                throw new ArgumentException("The room price must be greater than 0.");

            _mapper.Map(updateRoomDto, room);
            await _roomRepository.UpdateAsync(room);
        }

        public async Task DeleteRoomAsync(int id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null)
                throw new Exception("Room not found.");

            await _roomRepository.DeleteAsync(room);
        }
    }
}
