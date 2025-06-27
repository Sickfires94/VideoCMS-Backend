namespace Backend.DTOs.ResponseDtos
{
    // DTO to return user data along with the token
    public class AuthenticatedUserResponseDto : UserResponseDto
    {
        public string Token { get; set; } = string.Empty;
    }
}
