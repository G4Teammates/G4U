using System.ComponentModel.DataAnnotations;
using static Client.Models.Enum.UserEnum.User;

namespace Client.Models.CustomValidation
{
    public class BankInfoValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var instance = validationContext.ObjectInstance;
            var bankNameProperty = validationContext.ObjectType.GetProperty("BankName");
            var bankAccountProperty = validationContext.ObjectType.GetProperty("BankAccount");

            var bankNameValue = (BankName?)bankNameProperty.GetValue(instance);
            var bankAccountValue = (string?)bankAccountProperty.GetValue(instance);

            if (bankAccountValue != null && bankNameValue == BankName.Unknown) // Unknown là giá trị mặc định của enum BankName
                return new ValidationResult("Bank Name is required when input bank account.");
          
            if (bankAccountValue == null && bankNameValue != BankName.Unknown) // Unknown là giá trị mặc định của enum BankName
                return new ValidationResult("Bank Account is required when input bank name.");

            return ValidationResult.Success;
        }
    }

}
