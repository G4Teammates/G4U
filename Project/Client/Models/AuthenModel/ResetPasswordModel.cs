using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Client.Models.AuthenModel
{
    public class ResetPasswordModel
    {
        public string Token { get; set; }

        [Required(ErrorMessage = "Please enter the new password.")]
        [StringLength(64, ErrorMessage = "The new password must be at least {2} and at most {1} characters long.", MinimumLength = 6)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please confirm the password.")]
        [Compare(nameof(NewPassword), ErrorMessage = "The confirm password does not match.")]
        [StringLength(64, ErrorMessage = "The confirm password must be at least {2} and at most {1} characters long.", MinimumLength = 6)]
        [JsonIgnore]
        public string ConfirmPassword { get; set; }

    }
}
