using System.Net.Mail;
using System.Net;
using OrderMicroservice.Models;

namespace OrderMicroservice.Repositories.Interfaces
{
    public interface IHelperService
    {
        Task<ResponseModel> SendEmailAsync(string email, string subject, string htmlMessage);
       
    }
}
