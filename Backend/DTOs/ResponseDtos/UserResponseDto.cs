namespace Backend.DTOs.ResponseDtos
{
    public class UserResponseDto
    {
        public int userId { get; set; }
        public string userName { get; set; }
        public string userEmail { get; set; }
        public string role { get; set; }
        public DateTime userCreatedDate { get; set; }

    }
}
