﻿using Client.Models.Enum;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using static Client.Models.Enum.UserEnum.User;

namespace Client.Models.AuthenModel
{
    public class LoginGoogleRequestModel
    {
        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(320, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public string? Username { get; set; }
        [StringLength(256, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public string? DisplayName { get; set; }
        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(320, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [EmailAddress(ErrorMessage = "The {0} field is not a valid e-mail address.")]
        public string? Email { get; set; }
        [Url(ErrorMessage = "The {0} field is not a valid URL.")]
        [MaxLength(2048)]
        public string? Picture { get; set; }
        public EmailStatus EmailConfirmation { get; set; }
    }
}
