using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class User
    {
        public int userId {  get; set; }

        public string? userName { get; set; }

        [Required]
        public string userPassword { get; set; }

        [Required]
        public string userEmail { get; set; }

       public DateTime userCreatedDate { get; set; }
        public DateTime userUpdatedDate { get; set; }

    }
}
