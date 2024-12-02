using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UserMicroservice.Models.CustomValidation
{
    public class WhiteSpaceValidation : ValidationAttribute
    {
        public WhiteSpaceValidation() : base("{0} cannot have leading or trailing spaces and must not contain more than one consecutive space.") { }

        public override bool IsValid(object value)
        {
            if (value == null) return true;  // Cho phép giá trị null nếu bạn muốn kiểm tra trong controller

            string stringValue = value.ToString();

            // Loại bỏ khoảng trắng ở đầu và cuối, thay thế các khoảng trắng dư thừa giữa các từ bằng một khoảng trắng duy nhất
            stringValue = stringValue.Trim();

            // Kiểm tra nếu stringValue chỉ có khoảng trắng hoặc rỗng
            if (string.IsNullOrWhiteSpace(stringValue))
                return false;

            // Thay thế các khoảng trắng dư thừa giữa các từ thành một khoảng trắng duy nhất
            string cleanedstringValue = Regex.Replace(stringValue, @"\s+", " ");

            // Nếu cleanedstringValue có sự thay đổi so với input gốc thì là không hợp lệ
            return stringValue == cleanedstringValue;
        }
    }
}