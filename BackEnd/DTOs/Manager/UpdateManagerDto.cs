namespace BackEnd.DTOs.Manager
{
    public class UpdateManagerDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        // Email and PersonalNumber should not be changed this easily, so they are not requested here.
    }
}
