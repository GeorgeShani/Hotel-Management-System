using System.ComponentModel;

namespace WinForms.Models;

// Authentication response returned by /api/Auth/login and /api/Auth/register.
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
}

// View-model used by the Hotels tab (matches HotelDto / Create / Update shapes).
public class HotelModel
{
    [ReadOnly(true)]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    [Description("Rating from 1 to 5")]
    public int Rating { get; set; } = 5;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

// View-model used by the Rooms tab.
public class RoomModel
{
    [ReadOnly(true)]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    [Description("Id of the hotel this room belongs to")]
    public int HotelId { get; set; }
}

// View-model used by the Guests tab.
public class GuestModel
{
    [ReadOnly(true)]
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    [Description("Unique personal number (cannot be changed after creation)")]
    public string PersonalNumber { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    [Description("Login email – must match the account the guest signs in with so they can see their reservations")]
    public string Email { get; set; } = string.Empty;
}

// View-model used by the Managers tab.
public class ManagerModel
{
    [ReadOnly(true)]
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    [Description("Unique personal number (set only on creation)")]
    public string PersonalNumber { get; set; } = string.Empty;
    [Description("Unique email (set only on creation)")]
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int HotelId { get; set; }
}

// JSON shape exchanged with /api/Reservations (RoomIds is a list).
public class ReservationDtoJson
{
    public int Id { get; set; }
    public int GuestId { get; set; }
    public List<int> RoomIds { get; set; } = new();
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal TotalPrice { get; set; }
}

// View-model used by the Reservations tab. RoomIds is edited as a CSV string
// because PropertyGrid cannot conveniently edit a List<int>.
public class ReservationModel
{
    [ReadOnly(true)]
    public int Id { get; set; }
    public int GuestId { get; set; }
    [Description("Comma-separated room ids, e.g. 1,2,3")]
    public string RoomIds { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; } = DateTime.Today;
    public DateTime CheckOutDate { get; set; } = DateTime.Today.AddDays(1);
    [ReadOnly(true)]
    public decimal TotalPrice { get; set; }
}
