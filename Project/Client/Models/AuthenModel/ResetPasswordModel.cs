using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Client.Models.AuthenModel
{
    public class ResetPasswordModel
    {
        public string Token { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới đó")]
        [StringLength(64, ErrorMessage = "Mật khẩu mới phải ít nhất {2} và tối đa {1} kí tự.", MinimumLength = 6)]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập xác nhận mật khẩu")]
        [Compare(nameof(NewPassword), ErrorMessage = "Xác nhận mật khẩu không trùng khớp")]
        [StringLength(64, ErrorMessage = "Xác nhận mật khẩu phải ít nhất {2} và tối đa {1} kí tự.", MinimumLength = 6)]
        [JsonIgnore]
        public string ConfirmPassword { get; set; }
    }
}
