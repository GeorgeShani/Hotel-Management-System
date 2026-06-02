using AutoMapper;
using BackEnd.DTOs.Reservation;
using BackEnd.Interfaces;
using BackEnd.Models;

namespace BackEnd.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IGuestRepository _guestRepository;
        private readonly IMapper _mapper;

        public ReservationService(
            IReservationRepository reservationRepository,
            IRoomRepository roomRepository,
            IGuestRepository guestRepository,
            IMapper mapper)
        {
            _reservationRepository = reservationRepository;
            _roomRepository = roomRepository;
            _guestRepository = guestRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReservationDto>> GetAllReservationsAsync()
        {
            var reservations = await _reservationRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ReservationDto>>(reservations);
        }

        public async Task<ReservationDto?> GetReservationByIdAsync(int id)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);
            return reservation == null ? null : _mapper.Map<ReservationDto>(reservation);
        }

        public async Task<ReservationDto> CreateReservationAsync(CreateReservationDto createDto)
        {
            // 1. Validate the dates
            if (createDto.CheckInDate.Date < DateTime.UtcNow.Date)
                throw new ArgumentException("The check-in date cannot be in the past.");
            if (createDto.CheckOutDate <= createDto.CheckInDate)
                throw new ArgumentException("The check-out date must be later than the check-in date.");

            // 2. Validate the guest
            var guest = await _guestRepository.GetByIdAsync(createDto.GuestId);
            if (guest == null) throw new Exception("Guest not found.");

            // 3. Validate room existence and compute the total price
            decimal totalRoomsPrice = 0;
            var reservationRooms = new List<ReservationRoom>();

            foreach (var roomId in createDto.RoomIds)
            {
                var room = await _roomRepository.GetByIdAsync(roomId);
                if (room == null) throw new Exception($"Room with ID {roomId} not found.");

                totalRoomsPrice += room.Price;
                reservationRooms.Add(new ReservationRoom { RoomId = roomId }); // prepare the join-table entries
            }

            // 4. Check availability for all rooms at once
            bool areAvailable = await _reservationRepository.AreRoomsAvailableAsync(createDto.RoomIds, createDto.CheckInDate, createDto.CheckOutDate);
            if (!areAvailable)
                throw new InvalidOperationException("One or all of the selected rooms are already booked for these dates.");

            // 5. Create the reservation
            var reservationEntity = new Reservation
            {
                GuestId = createDto.GuestId,
                CheckInDate = createDto.CheckInDate,
                CheckOutDate = createDto.CheckOutDate,
                ReservationRooms = reservationRooms // attach the list of rooms
            };

            var createdReservation = await _reservationRepository.AddAsync(reservationEntity);

            // 6. Return the DTO
            int totalDays = (createDto.CheckOutDate - createDto.CheckInDate).Days;
            if (totalDays == 0) totalDays = 1;

            return new ReservationDto
            {
                Id = createdReservation.Id,
                GuestId = createdReservation.GuestId,
                RoomIds = createDto.RoomIds,
                CheckInDate = createdReservation.CheckInDate,
                CheckOutDate = createdReservation.CheckOutDate,
                TotalPrice = totalDays * totalRoomsPrice
            };
        }

        public async Task UpdateReservationAsync(int id, UpdateReservationDto updateDto)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null) throw new Exception("Reservation not found.");

            if (updateDto.CheckOutDate <= updateDto.CheckInDate)
                throw new ArgumentException("The check-out date must be later than the check-in date.");

            _mapper.Map(updateDto, reservation);
            await _reservationRepository.UpdateAsync(reservation);
        }

        public async Task DeleteReservationAsync(int id)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null) throw new Exception("Reservation not found.");

            await _reservationRepository.DeleteAsync(reservation);
        }
    }
}
