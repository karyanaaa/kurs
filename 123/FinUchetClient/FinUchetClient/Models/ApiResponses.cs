namespace FinUchetClient.Models
{
    public class LoginResponse
    {
        public string access_token { get; set; } = string.Empty;
        public string token_type { get; set; } = string.Empty;
    }

    public class RegisterResponse
    {
        public string msg { get; set; } = string.Empty;
    }

    public class ErrorResponse
    {
        public string detail { get; set; } = string.Empty;
    }
}