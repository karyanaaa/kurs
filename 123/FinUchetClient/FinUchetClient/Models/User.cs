namespace FinUchetClient.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string HashedPassword { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
    }
}