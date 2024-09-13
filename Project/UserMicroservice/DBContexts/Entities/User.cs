using Microsoft.AspNetCore.Identity;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using UserMicroService.DBContexts.Enum;

namespace UserMicroservice.DBContexts.Entities
{
    #region SQL
    //public class User : IdentityUser<Guid>
    //{
    //    [Required(ErrorMessage = "Email is required")]
    //    public override string? UserName { get; set; }
    //    [Phone(ErrorMessage = "Phone number must be correct format")]
    //    [MaxLength(15, ErrorMessage = "Phone number must be less than 15 digits")]
    //    public override string? PhoneNumber { get; set; }
    //    [Required(ErrorMessage = "Email is required")]
    //    [EmailAddress(ErrorMessage = "Email must be correct format")]
    //    [MaxLength(320, ErrorMessage = "Email must be less than 320 characters")]
    //    public override string? Email { get; set; }
    //    [MaxLength(256)]
    //    public string? DisplayName { get; set; }
    //    public string? Avatar { get; set; }

    //    public override string? NormalizedEmail => Email?.ToUpper();
    //    public override string? NormalizedUserName => UserName?.ToUpper();

    //    public required UserStatus Status { get; set; }
    //    public DateTime CreatedAt { get; set; }
    //    public DateTime? UpdatedAt { get; set; }
    #endregion


    #region noSQL
    /// <summary>
    /// Represents a user in the system.
    /// <br/>
    /// Đại diện cho một người dùng trong hệ thống.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier for the user.
        /// <br/>
        /// Định danh duy nhất cho người dùng.
        /// </summary>
        [BsonId]
        [BsonElement("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// The username of the user.
        /// <br/>
        /// Tên người dùng.
        /// </summary>
        [BsonElement("userName")]
        public required string UserName { get; set; }

        /// <summary>
        /// The phone number of the user.
        /// <br/>
        /// Số điện thoại của người dùng.
        /// </summary>
        [BsonElement("phoneNumber")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// The email address of the user.
        /// <br/>
        /// Địa chỉ email của người dùng.
        /// </summary>
        [BsonElement("email")]
        public required string Email { get; set; }

        /// <summary>
        /// The display name of the user.
        /// <br/>
        /// Tên hiển thị của người dùng.
        /// </summary>
        [BsonElement("displayName")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// The avatar URL of the user.
        /// <br/>
        /// URL của ảnh đại diện người dùng.
        /// </summary>
        [BsonElement("avatar")]
        public string? Avatar { get; set; }

        /// <summary>
        /// The email confirmation status (e.g., not confirmed (0), confirmed (1)).
        /// <br/>
        /// Trạng thái xác nhận email (ví dụ: chưa xác nhận (0), đã xác nhận (1)).
        /// </summary>
        [BsonElement("emailConfirmation")]
        public EmailStatus EmailConfirmation { get; set; }

        /// <summary>
        /// The hashed password of the user.
        /// <br/>
        /// Mật khẩu đã băm của người dùng.
        /// </summary>
        [BsonElement("passwordHash")]
        public string? PasswordHash { get; set; }

        /// <summary>
        /// The normalized email address of the user (uppercase).
        /// <br/>
        /// Địa chỉ email chuẩn hóa của người dùng (là chữ hoa).
        /// </summary>
        [BsonElement("normalizedEmail")]
        public string? NormalizedEmail { get; set; }

        /// <summary>
        /// The normalized username of the user (uppercase).
        /// <br/>
        /// Tên người dùng chuẩn hóa của người dùng (là chữ hoa).
        /// </summary>
        [BsonElement("normalizedUserName")]
        public string? NormalizedUserName { get; set; }

        /// <summary>
        /// The status of the user account.
        /// <br/>
        /// Trạng thái của tài khoản người dùng.
        /// </summary>
        [BsonElement("status")]
        public required UserStatus Status { get; set; }

        /// <summary>
        /// The date and time when the user account was created.
        /// <br/>
        /// Ngày và giờ khi tài khoản người dùng được tạo.
        /// </summary>
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The date and time when the user account was last updated.
        /// <br/>
        /// Ngày và giờ khi tài khoản người dùng được cập nhật lần cuối.
        /// </summary>
        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }

    #endregion
}