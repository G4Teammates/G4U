using System.ComponentModel.DataAnnotations;
using UserMicroservice.DBContexts.Enum;
using UserMicroservice.Models.CustomValidation;

namespace UserMicroservice.Models.UserManagerModel
{
    public class UserUpdate
    {
        /// <summary>
        /// Unique identifier for the user.
        /// <br/>
        /// Định danh duy nhất cho người dùng.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// The username of the user.
        /// <br/>
        /// Tên người dùng.
        /// </summary>
        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(320, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [RegularExpression(@"^[a-zA-Z0-9_@.-]*$", ErrorMessage = "The {0} can only contain letters, numbers, underscores (_), at (@), dot(.) and hyphens (-).")]
        public required string Username { get; set; }

        /// <summary>
        /// The normalized username of the user (uppercase).
        /// <br/>
        /// Tên người dùng chuẩn hóa của người dùng (là chữ hoa).
        /// </summary>
        public string NormalizedUsername => Username.ToUpper();



        [StringLength(15, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 7)]
        [RegularExpression(@"^\d+$", ErrorMessage = "The {0} field can only contain numbers.")]
        [Phone(ErrorMessage = "The {0} field is not a valid phone number.")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// The display name of the user.
        /// <br/>
        /// Tên hiển thị của người dùng.
        /// </summary>
        private string? _displayName;
        [StringLength(256, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [RegularExpression(@"^(?!\s)(?!.*\s{2,}).*(?<!\s)$", ErrorMessage = "The input must not have leading or trailing spaces, and must not contain more than one consecutive space.")]
        public string? DisplayName
        {
            get => string.IsNullOrEmpty(_displayName) ? Username : _displayName;
            set => _displayName = value;
        }

        /// <summary>
        /// The email address of the user.
        /// <br/>
        /// Địa chỉ email của người dùng.
        /// </summary>
        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(320, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [EmailAddress(ErrorMessage = "The {0} field is not a valid e-mail address.")]
        public required string Email { get; set; }

        /// <summary>
        /// The normalized email address of the user (uppercase).
        /// <br/>
        /// Địa chỉ email chuẩn hóa của người dùng (là chữ hoa).
        /// </summary>
        public string NormalizedEmail => Email.ToUpper();

        /// <summary>
        /// The Bank Account of the user.
        /// <br/>
        /// Số tài khoản của người dùng.
        /// </summary>
        [StringLength(17, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 5)]
        [RegularExpression("^[0-9]+$", ErrorMessage = "The {0} must be number.")]
        public string? BankAccount { get; set; }

        /// <summary>
        /// The avatar URL of the user.
        /// <br/>
        /// URL của ảnh đại diện người dùng.
        /// </summary>
        [Url(ErrorMessage = "The {0} field is not a valid URL.")]
        [MaxLength(2048)]
        public string Avatar { get; set; } = "https://static.vecteezy.com/system/resources/previews/020/911/747/non_2x/user-profile-icon-profile-avatar-user-icon-male-icon-face-icon-profile-icon-free-png.png";

        /// <summary>
        /// The role of the user account, include <see cref="UserRole.User"/>,<see cref="UserRole.Admin"/>,<see cref="UserRole.Developer"/>.
        /// <br/>
        /// Vai trò của tài khoản người dùng, bao gồm <see cref="UserRole.User"/>(người dùng mặc định), <see cref="UserRole.Admin"/>(quản trị viên) và <see cref="UserRole.Developer"/>(lập trình viên).
        /// </summary>
        public UserRole? Role { get; set; } = UserRole.User;

        public UserStatus? Status { get; set; } = UserStatus.Inactive;

        public EmailStatus? EmailConfirmation { get; set; } = EmailStatus.Unconfirmed;

        public DateTime UpdateAt { get; set; }
    }
}
