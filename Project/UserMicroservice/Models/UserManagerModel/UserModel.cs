﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.DBContexts.Enum;
using UserMicroservice.Models.CustomValidation;

namespace UserMicroservice.Models.UserManagerModel
{
    public class UserModel
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
        /// The phone number of the user.
        /// <br/>
        /// Số điện thoại của người dùng.
        /// </summary>
        [StringLength(15, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 7)]
        [Phone(ErrorMessage = "The {0} field is not a valid phone number.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "The {0} field can only contain numbers.")]
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
        /// The Bank Name of the user.
        /// <br/>
        /// Tên ngân hàng của người dùng.
        /// </summary>
        public BankName BankName { get; set; }

        /// <summary>
        /// The Bank Account of the user.
        /// <br/>
        /// Số tài khoản của người dùng.
        /// </summary>
        [StringLength(17, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [RegularExpression(@"^\d+$", ErrorMessage = "The {0} field can only contain numbers.")]

        public string? BankAccount { get; set; }
        /// <summary>
        /// The type of login user account, include <see cref="UserLoginType.Local"/>(login with username and password),<see cref="UserLoginType.Google"/> or <see cref="UserLoginType.Other"/>.
        /// <br/>
        /// Vai trò của tài khoản người dùng, bao gồm <see cref="UserLoginType.Local"/>(đăng nhập bằng username và password),<see cref="UserLoginType.Google"/> hoặc <see cref="UserLoginType.Other"/>.
        /// </summary>
        public UserLoginType LoginType { get; set; }

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
        public UserRole Role { get; set; } = UserRole.User;

        /// <summary>
        /// The email confirmation status (e.g., not confirmed (0), confirmed (1)).
        /// <br/>
        /// Trạng thái xác nhận email (ví dụ: chưa xác nhận (0), đã xác nhận (1)).
        /// </summary>
        public EmailStatus EmailConfirmation { get; set; }

        /// <summary>
        /// The total profit earned by the user from selling games. 
        /// <br/>
        /// Tổng số tiền lời cuối cùng mà người dùng kiếm được từ việc bán game.
        /// </summary>
        public decimal TotalProfit { get; set; } = 0;

        /// <summary>
        /// Indicates whether the "Remember Me" option is selected for the user.
        /// <br/>
        /// Cho biết tùy chọn "Ghi nhớ đăng nhập" có được người dùng chọn hay không.
        /// </summary>
        [NotMapped]
        public bool IsRememberMe { get; set; } = false;

        /// <summary>
        /// The collection of games added by the user to their wishlist.
        /// <br/>
        /// Danh sách các game được người dùng thêm vào danh sách yêu thích.
        /// </summary>
        public ICollection<UserWishlistModel>? Wishlist { get; set; }

        /// <summary>
        /// The status of the user account.
        /// <br/>
        /// Trạng thái của tài khoản người dùng.
        /// </summary>
        public UserStatus Status { get; set; } = UserStatus.Inactive;

        /// <summary>
        /// The date and time when the user account was created.
        /// <br/>
        /// Ngày và giờ khi tài khoản người dùng được tạo.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The date and time when the user account was last updated.
        /// <br/>
        /// Ngày và giờ khi tài khoản người dùng được cập nhật lần cuối.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }

}
