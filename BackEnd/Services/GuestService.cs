using AutoMapper;
using BackEnd.DTOs.Guest;
using BackEnd.Interfaces;
using BackEnd.Models;

namespace BackEnd.Services
{
    public class GuestService : IGuestService
    {
        private readonly IGuestRepository _guestRepository;
        private readonly IMapper _mapper;

        public GuestService(IGuestRepository guestRepository, IMapper mapper)
        {
            _guestRepository = guestRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<GuestDto>> GetAllGuestsAsync()
        {
            var guests = await _guestRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<GuestDto>>(guests);
        }

        public async Task<GuestDto?> GetGuestByIdAsync(int id)
        {
            var guest = await _guestRepository.GetByIdAsync(id);
            return guest == null ? null : _mapper.Map<GuestDto>(guest);
        }

        public async Task<GuestDto> CreateGuestAsync(CreateGuestDto createGuestDto)
        {
            // Validation: check whether a guest with this personal number already exists
            if (await _guestRepository.PersonalNumberExistsAsync(createGuestDto.PersonalNumber))
            {
                // The middleware catches this and returns 400 Bad Request
                throw new InvalidOperationException("A guest with this personal number already exists in the system.");
            }

            var guestEntity = _mapper.Map<Guest>(createGuestDto);
            var createdGuest = await _guestRepository.AddAsync(guestEntity);

            return _mapper.Map<GuestDto>(createdGuest);
        }

        public async Task UpdateGuestAsync(int id, UpdateGuestDto updateGuestDto)
        {
            var guest = await _guestRepository.GetByIdAsync(id);
            if (guest == null)
                throw new Exception("Guest not found.");

            _mapper.Map(updateGuestDto, guest);
            await _guestRepository.UpdateAsync(guest);
        }

        public async Task DeleteGuestAsync(int id)
        {
            var guest = await _guestRepository.GetByIdAsync(id);
            if (guest == null)
                throw new Exception("Guest not found.");

            await _guestRepository.DeleteAsync(guest);
        }
    }
}
