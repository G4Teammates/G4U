using OrderMicroservice.Repositories.Interfaces;
using System.Net.Mail;
using System.Net;
using OrderMicroservice.Models;
using Microsoft.Extensions.Configuration;

namespace OrderMicroservice.Repositories.Services
{
    public class HelperService(IConfiguration configuration) : IHelperService
    {
        private readonly IConfiguration _configuration = configuration;
        public async Task<ResponseModel> SendEmailAsync(string email, string subject, string htmlMessage)
        {
            ResponseModel response = new();
            try
            {
                var emailstring = _configuration["21"];
                var SenderPassword = _configuration["22"];
                using var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(emailstring, SenderPassword),
                    EnableSsl = true
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailstring),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);
                response.Message = "Email sent successfully";
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.IsSuccess = false;
            }
            return response;
        }


    }
}
