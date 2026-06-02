using AutoMapper;
using BackEnd.DTOs.Manager;
using BackEnd.Interfaces;
using BackEnd.Models;

namespace BackEnd.Services
{
    public class ManagerService : IManagerService
    {
        private readonly IManagerRepository _managerRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IMapper _mapper;

        public ManagerService(
            IManagerRepository managerRepository,
            IHotelRepository hotelRepository,
            IMapper mapper)
        {
            _managerRepository = managerRepository;
            _hotelRepository = hotelRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ManagerDto>> GetAllManagersAsync()
        {
            var managers = await _managerRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ManagerDto>>(managers);
        }

        public async Task<ManagerDto?> GetManagerByIdAsync(int id)
        {
            var manager = await _managerRepository.GetByIdAsync(id);
            return manager == null ? null : _mapper.Map<ManagerDto>(manager);
        }

        public async Task<ManagerDto> CreateManagerAsync(CreateManagerDto createDto)
        {
            // 1. Check whether the hotel exists
            var hotel = await _hotelRepository.GetByIdAsync(createDto.HotelId);
            if (hotel == null) throw new Exception("Hotel not found.");

            // 2. Check email uniqueness
            if (await _managerRepository.EmailExistsAsync(createDto.Email))
                throw new InvalidOperationException("A manager with this email already exists.");

            // 3. Check personal number uniqueness
            if (await _managerRepository.PersonalNumberExistsAsync(createDto.PersonalNumber))
                throw new InvalidOperationException("A manager with this personal number already exists.");

            var managerEntity = _mapper.Map<Manager>(createDto);
            var createdManager = await _managerRepository.AddAsync(managerEntity);

            return _mapper.Map<ManagerDto>(createdManager);
        }

        public async Task UpdateManagerAsync(int id, UpdateManagerDto updateDto)
        {
            var manager = await _managerRepository.GetByIdAsync(id);
            if (manager == null) throw new Exception("Manager not found.");

            _mapper.Map(updateDto, manager);
            await _managerRepository.UpdateAsync(manager);
        }

        public async Task DeleteManagerAsync(int id)
        {
            var manager = await _managerRepository.GetByIdAsync(id);
            if (manager == null) throw new Exception("Manager not found.");

            // Core business rule: check whether the hotel has another manager
            int managerCount = await _managerRepository.GetManagersCountByHotelIdAsync(manager.HotelId);
            if (managerCount <= 1)
                throw new InvalidOperationException("The manager cannot be deleted because they are the only one for this hotel.");

            await _managerRepository.DeleteAsync(manager);
        }
    }
}
