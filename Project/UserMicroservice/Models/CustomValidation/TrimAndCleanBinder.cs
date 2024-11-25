using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.RegularExpressions;

namespace UserMicroservice.Models.CustomValidation
{
    public class TrimAndCleanBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.FieldName).FirstValue;

            if (!string.IsNullOrEmpty(value))
            {
                value = Regex.Replace(value.Trim(), @"\s+", " ");
            }

            bindingContext.Result = ModelBindingResult.Success(value);
            return Task.CompletedTask;
        }
    }

}
