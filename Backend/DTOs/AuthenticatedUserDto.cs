namespace Backend.DTOs
{
    // DTO to return user data along with the token
    public class AuthenticatedUserDto : User
    {
        public string Token { get; set; } = string.Empty;
    }
}
