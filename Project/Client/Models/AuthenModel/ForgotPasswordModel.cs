using System.ComponentModel.DataAnnotations;

namespace Client.Models.AuthenModel
{
    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(320, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [EmailAddress(ErrorMessage = "The {0} field is not a valid e-mail address.")]
        public string? Email { get; set; }
    }
}
