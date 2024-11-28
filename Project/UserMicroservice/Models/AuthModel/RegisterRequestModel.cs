using System.ComponentModel.DataAnnotations;
using UserMicroservice.Models.CustomValidation;

namespace UserMicroservice.Models.AuthModel
{
    public class RegisterRequestModel
    {
        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(320, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [RegularExpression(@"^[a-zA-Z0-9_@-]*$", ErrorMessage = "The {0} can only contain letters, numbers, underscores (_), at (@) and hyphens (-).")]
        public required string Username { get; set; }
        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(320, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [EmailAddress(ErrorMessage = "The {0} field is not a valid e-mail address.")]
        public required string Email { get; set; }
        [StringLength(64, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public required string Password { get; set; }
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
        public required string ConfirmPassword { get; set; }
    }
}
