namespace Backend.DTOs.RequestDtos
{
    public class UserRequestDto
    {
        public string userName {  get; set; }
        public string userPassword { get; set; }
        public string userEmail { get; set; }
        public string? role { get; set; }

    }
}
