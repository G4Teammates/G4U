using System.ComponentModel.DataAnnotations;
using UserMicroservice.Models.CustomValidation;

namespace UserMicroservice.Models.AuthModel
{
    public class LoginRequestModel
    {
        [WhiteSpaceValidation(ErrorMessage = "{0} cannot have leading or trailing spaces and must not contain more than one consecutive space.")]
        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(320, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public required string UsernameOrEmail { get; set; }
        [StringLength(64, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public required string Password { get; set; }

    }
}
