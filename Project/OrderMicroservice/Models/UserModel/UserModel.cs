﻿using OrderMicroservice.Models.UserModel.Enum;
using System.ComponentModel.DataAnnotations;

namespace OrderMicroservice.Models.UserModel
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
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// The display name of the user.
        /// <br/>
        /// Tên hiển thị của người dùng.
        /// </summary>
        [StringLength(256, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        private string? _displayName;
        public string? DisplayName
        {
            get => string.IsNullOrEmpty(_displayName) ? Username : _displayName;
            set => _displayName = value;
        }

        /// <summary>
        /// The avatar URL of the user.
        /// <br/>
        /// URL của ảnh đại diện người dùng.
        /// </summary>
        [Url(ErrorMessage = "The {0} field is not a valid URL.")]
        [MaxLength(2048)]
        public string Avatar { get; set; } = "https://static.vecteezy.com/system/resources/previews/020/911/747/non_2x/user-profile-icon-profile-avatar-user-icon-male-icon-face-icon-profile-icon-free-png.png";

        /// <summary>
        /// The total profit earned by the user from selling games. 
        /// <br/>
        /// Tổng số tiền lời cuối cùng mà người dùng kiếm được từ việc bán game.
        /// </summary>
        public decimal TotalProfit { get; set; } = 0;
    }

}
