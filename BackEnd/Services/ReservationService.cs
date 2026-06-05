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

        // A reservation "belongs to" a signed-in guest when the guest record it
        // points to carries the same email the user logged in with.
        private static bool OwnedBy(Reservation reservation, string userEmail) =>
            !string.IsNullOrWhiteSpace(userEmail) &&
            string.Equals(reservation.Guest?.Email, userEmail, StringComparison.OrdinalIgnoreCase);

        public async Task<IEnumerable<ReservationDto>> GetAllReservationsAsync(string userEmail, bool isPrivileged)
        {
            var reservations = await _reservationRepository.GetAllAsync();

            // Guests only see reservations made for them (matched by their login email).
            if (!isPrivileged)
                reservations = reservations.Where(r => OwnedBy(r, userEmail)).ToList();

            return _mapper.Map<IEnumerable<ReservationDto>>(reservations);
        }

        public async Task<ReservationDto?> GetReservationByIdAsync(int id, string userEmail, bool isPrivileged)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null) return null;

            if (!isPrivileged && !OwnedBy(reservation, userEmail))
                throw new UnauthorizedAccessException("You can only access your own reservations.");

            return _mapper.Map<ReservationDto>(reservation);
        }

        public async Task<ReservationDto> CreateReservationAsync(CreateReservationDto createDto, string userEmail, bool isPrivileged)
        {
            // 1. Validate the dates
            if (createDto.CheckInDate.Date < DateTime.UtcNow.Date)
                throw new ArgumentException("The check-in date cannot be in the past.");
            if (createDto.CheckOutDate <= createDto.CheckInDate)
                throw new ArgumentException("The check-out date must be later than the check-in date.");

            // 2. Validate the guest
            var guest = await _guestRepository.GetByIdAsync(createDto.GuestId);
            if (guest == null) throw new Exception("Guest not found.");

            // A non-privileged caller (Guest role) may only book for their own guest
            // record - i.e. the one whose email matches their login.
            if (!isPrivileged &&
                !string.Equals(guest.Email, userEmail, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("You can only create reservations for yourself.");

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

        public async Task UpdateReservationAsync(int id, UpdateReservationDto updateDto, string userEmail, bool isPrivileged)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null) throw new Exception("Reservation not found.");

            if (!isPrivileged && !OwnedBy(reservation, userEmail))
                throw new UnauthorizedAccessException("You can only modify your own reservations.");

            if (updateDto.CheckOutDate <= updateDto.CheckInDate)
                throw new ArgumentException("The check-out date must be later than the check-in date.");

            _mapper.Map(updateDto, reservation);
            await _reservationRepository.UpdateAsync(reservation);
        }

        public async Task DeleteReservationAsync(int id, string userEmail, bool isPrivileged)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);
            if (reservation == null) throw new Exception("Reservation not found.");

            if (!isPrivileged && !OwnedBy(reservation, userEmail))
                throw new UnauthorizedAccessException("You can only delete your own reservations.");

            await _reservationRepository.DeleteAsync(reservation);
        }
    }
}
