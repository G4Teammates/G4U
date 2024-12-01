using OrderMicroservice.Repositories.Interfaces;
using System.Net.Mail;
using System.Net;
using OrderMicroservice.Models;

namespace OrderMicroservice.Repositories.Services
{
    public class HelperService : IHelperService
    {
        public async Task<ResponseModel> SendEmailAsync(string email, string subject, string htmlMessage)
        {
            ResponseModel response = new();
            try
            {
                using var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("kiet43012@gmail.com", "fjrq yuus fmaf ugbt"),
                    EnableSsl = true
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress("kiet43012@gmail.com"),
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
