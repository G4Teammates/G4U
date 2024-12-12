using System.ComponentModel.DataAnnotations;
using UserMicroservice.DBContexts.Enum;
using UserMicroservice.Models.CustomValidation;

namespace UserMicroservice.Models.AuthModel
{
    public class LoginRequestModel
    {
        [RegularExpression(@"^[a-zA-Z0-9_@.-]*$", ErrorMessage = "The {0} can only contain letters, numbers, underscores (_), at (@), dot(.) and hyphens (-).")]
        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(320, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public required string UsernameOrEmail { get; set; }
        [StringLength(64, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public required string Password { get; set; }
        //public UserLoginType LoginType { get; set; }
        public bool IsRememberMe { get; set; } = false;
    }
}