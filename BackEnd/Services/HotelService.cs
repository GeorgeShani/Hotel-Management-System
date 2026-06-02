using AutoMapper;
using BackEnd.DTOs.Hotel;
using BackEnd.Interfaces;
using BackEnd.Models;

namespace BackEnd.Services
{
    public class HotelService : IHotelService
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IMapper _mapper;

        // Repository and Mapper are injected via Dependency Injection
        public HotelService(IHotelRepository hotelRepository, IMapper mapper)
        {
            _hotelRepository = hotelRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<HotelDto>> GetHotelsAsync(string? country, string? city, int? rating)
        {
            // Fetch the filtered data from the database
            var hotels = await _hotelRepository.GetFilteredHotelsAsync(country, city, rating);

            // Return the list converted to DTOs
            return _mapper.Map<IEnumerable<HotelDto>>(hotels);
        }

        public async Task<HotelDto?> GetHotelByIdAsync(int id)
        {
            var hotel = await _hotelRepository.GetByIdAsync(id);
            if (hotel == null) return null;

            return _mapper.Map<HotelDto>(hotel);
        }

        public async Task<HotelDto> CreateHotelAsync(CreateHotelDto createHotelDto)
        {
            // Convert the DTO into a Hotel entity
            var hotelEntity = _mapper.Map<Hotel>(createHotelDto);

            // Persist to the database
            var createdHotel = await _hotelRepository.AddAsync(hotelEntity);

            // Convert back to a DTO (now with the assigned ID)
            return _mapper.Map<HotelDto>(createdHotel);
        }

        public async Task UpdateHotelAsync(int id, UpdateHotelDto updateHotelDto)
        {
            var hotel = await _hotelRepository.GetByIdAsync(id);
            if (hotel == null)
                throw new Exception("Hotel not found.");

            // AutoMapper applies the new data onto the existing object
            _mapper.Map(updateHotelDto, hotel);

            await _hotelRepository.UpdateAsync(hotel);
        }

        public async Task DeleteHotelAsync(int id)
        {
            var hotel = await _hotelRepository.GetByIdAsync(id);
            if (hotel == null)
                throw new Exception("Hotel not found.");

            // Business-logic validation: does it have rooms?
            bool hasRooms = await _hotelRepository.HasRoomsAsync(id);
            if (hasRooms)
            {
                throw new InvalidOperationException("The hotel cannot be deleted because it has rooms.");
            }

            // If it has no rooms, delete it
            await _hotelRepository.DeleteAsync(hotel);
        }
    }
}
