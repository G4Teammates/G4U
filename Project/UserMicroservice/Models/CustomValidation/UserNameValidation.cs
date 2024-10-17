using System.ComponentModel.DataAnnotations;
using UserMicroservice.DBContexts.Entities;

namespace UserMicroservice.Models.CustomValidation
{
    public class UserNameValidation : ValidationAttribute
    {
        //Example of Custom Validation
        //public static ValidationResult ValidateUserName(string userName)
        //{
        //    if (userName.Length < 6)
        //    {
        //        return new ValidationResult("User name must be at least 6 characters long");
        //    }
        //    return ValidationResult.Success;
        //}
    }
}
