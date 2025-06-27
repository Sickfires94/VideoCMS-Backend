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

        private string _role = "user";

        [Required]
        public string role
        {
            get => _role;
            set => _role = string.IsNullOrWhiteSpace(value) ? "user" : value;
        }

        public DateTime userCreatedDate { get; set; }
        public DateTime userUpdatedDate { get; set; }

    }
}
