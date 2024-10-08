using System.ComponentModel.DataAnnotations;

namespace UserMicroservice.Models
{
    public class LoginRequestModel
    {
        private string _usernameOrEmail;

        [Required(ErrorMessage = "The {0} field is required.")]
        public string UsernameOrEmail
        {
            get => _usernameOrEmail;
            set => _usernameOrEmail = value.ToUpper();
        }

        [Required(ErrorMessage = "The {0} field is required.")]
        public required string Password { get; set; }
    }
}
