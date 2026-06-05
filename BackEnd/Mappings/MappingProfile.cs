using AutoMapper;
using BackEnd.DTOs.Guest;
using BackEnd.DTOs.Hotel;
using BackEnd.DTOs.Manager;
using BackEnd.DTOs.Reservation;
using BackEnd.DTOs.Room;
using BackEnd.Models;

namespace BackEnd.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Hotel Mappings
            CreateMap<BackEnd.Models.Hotel, HotelDto>().ReverseMap();
            CreateMap<CreateHotelDto, BackEnd.Models.Hotel>();
            CreateMap<UpdateHotelDto, BackEnd.Models.Hotel>();

            // Room Mappings
            CreateMap<BackEnd.Models.Room, RoomDto>().ReverseMap();
            CreateMap<CreateRoomDto, BackEnd.Models.Room>();
            CreateMap<UpdateRoomDto, BackEnd.Models.Room>();

            // Guest Mappings
            CreateMap<BackEnd.Models.Guest, GuestDto>().ReverseMap();
            CreateMap<CreateGuestDto, BackEnd.Models.Guest>();
            CreateMap<UpdateGuestDto, BackEnd.Models.Guest>();

            // Reservation Mappings
            CreateMap<BackEnd.Models.Reservation, ReservationDto>()
                // RoomIds comes from the join table
                .ForMember(d => d.RoomIds,
                    opt => opt.MapFrom(s => s.ReservationRooms.Select(rr => rr.RoomId).ToList()))
                // TotalPrice = (nights, min 1) * sum of the booked rooms' prices
                .ForMember(d => d.TotalPrice,
                    opt => opt.MapFrom(s =>
                        (decimal)(((s.CheckOutDate - s.CheckInDate).Days <= 0) ? 1 : (s.CheckOutDate - s.CheckInDate).Days)
                        * s.ReservationRooms.Sum(rr => rr.Room.Price)));
            CreateMap<CreateReservationDto, BackEnd.Models.Reservation>();
            CreateMap<UpdateReservationDto, BackEnd.Models.Reservation>();

            // Manager Mappings
            CreateMap<BackEnd.Models.Manager, ManagerDto>().ReverseMap();
            CreateMap<CreateManagerDto, BackEnd.Models.Manager>();
            CreateMap<UpdateManagerDto, BackEnd.Models.Manager>();
        }
    }
}
