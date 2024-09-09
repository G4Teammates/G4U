using System.ComponentModel.DataAnnotations;
using User.DBContexts.Entities;

namespace UserMicroService.Models.CustomeValidation
{
    public class UserNameValidation : ValidationAttribute
    {
        public static ValidationResult ValidateUserName(string userName)
        {
            if (userName.Length < 6)
            {
                return new ValidationResult("User name must be at least 6 characters long");
            }
            return ValidationResult.Success;
        }
    }
}
