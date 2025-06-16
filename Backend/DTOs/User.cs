using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class User
    {
        public int userId {  get; set; }

        [Required]
        public string userName { get; set; }

        [Required]
        public string userPassword { get; set; }

        [Required]
        public string userEmail { get; set; }

       public DateOnly userCreatedDate { get; set; }
        public DateOnly userUpdatedDate { get; set; }

    }
}
