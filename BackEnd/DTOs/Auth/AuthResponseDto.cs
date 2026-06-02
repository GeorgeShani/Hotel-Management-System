namespace BackEnd.DTOs.Auth
{
    // What we return back (token and status)
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
    }
}
