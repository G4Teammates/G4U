﻿namespace UserMicroservice.Models.AuthModel
{
    public class ResetPasswordModel
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}