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

            if (bankNameProperty == null || bankAccountProperty == null)
                return new ValidationResult("Invalid properties for validation.");

            var bankNameValue = (BankName?)bankNameProperty.GetValue(instance);
            var bankAccountValue = (string?)bankAccountProperty.GetValue(instance);

            if (bankNameValue == null || bankNameValue == BankName.Unknown) // None là giá trị mặc định của enum BankName
                return new ValidationResult("Bank Name is required and must be valid.");

            if (string.IsNullOrWhiteSpace(bankAccountValue))
                return new ValidationResult("Bank Account is required.");

            return ValidationResult.Success;
        }
    }

}
