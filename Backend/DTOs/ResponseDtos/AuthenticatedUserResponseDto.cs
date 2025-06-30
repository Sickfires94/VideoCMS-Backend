namespace Backend.DTOs.ResponseDtos
{
    // DTO to return user data along with the token
    public class AuthenticatedUserResponseDto : UserResponseDto
    {
        public string token { get; set; } = string.Empty;
    }
}
