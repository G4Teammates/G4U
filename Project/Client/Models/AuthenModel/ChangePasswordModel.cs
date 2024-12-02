using System.ComponentModel.DataAnnotations;

namespace Client.Models.AuthenModel
{
    public class ChangePasswordModel : IValidatableObject
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Please enter the current password.")]
        [StringLength(64, ErrorMessage = "The current password must be at least {2} and at most {1} characters long.", MinimumLength = 6)]
        public string? OldPassword { get; set; }

        [Required(ErrorMessage = "Please enter the new password.")]
        [StringLength(64, ErrorMessage = "The new password must be at least {2} and at most {1} characters long.", MinimumLength = 6)]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "Please enter the password confirmation.")]
        [Compare(nameof(NewPassword), ErrorMessage = "The password confirmation does not match.")]
        [StringLength(64, ErrorMessage = "The password confirmation must be at least {2} and at most {1} characters long.", MinimumLength = 6)]
        public string? ConfirmPassword { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(OldPassword) && OldPassword == NewPassword)
            {
                yield return new ValidationResult(
                    "The new password cannot be the same as the old password.",
                    new[] { nameof(NewPassword) });
            }
        }
    }
}
