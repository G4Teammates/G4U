using AutoMapper;
using Azure.Core;
using Microsoft.Azure.CognitiveServices.ContentModerator.Models;
using Microsoft.EntityFrameworkCore;
using ReportMicroservice.DBContexts;
using ReportMicroservice.DBContexts.Entities;
using ReportMicroservice.DBContexts.Enum;
using ReportMicroservice.Models;
using System.Net.Mail;
using System.Net;
using X.PagedList.Extensions;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace ReportMicroservice.Repostories
{
    public class Repo : IRepo
    {
        private readonly ReportDbContext _db;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public Repo(ReportDbContext db, IMapper mapper, IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
        }
        public async Task<ResponseDTO> GetAll(int page, int pageSize)
        {
            ResponseDTO response = new();
            try
            {
                var Pros = await _db.reports.ToListAsync();
                if (Pros != null)
                {
                    response.Result = _mapper.Map<ICollection<Reports>>(Pros).ToPagedList(page, pageSize);
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Reports";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<ResponseDTO> CreateReport(CreateReportsModels model, string UserName)
        {
            ResponseDTO response = new();
            try
            {
                if (model != null)
                {
                    var newReport = new ReportsModel
                    {
                        UserName = UserName,
                        Description = model.Description,
                        Related = model.Related,
                        Email = model.Email,
                        CreatedAt = DateTime.UtcNow,
                        Status = (ReportsStatus)1
                    };
                    var Enti = _mapper.Map<Reports>(newReport);
                    _db.AddAsync(Enti);
                    _db.SaveChangesAsync();
                    response.Result = Enti;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Reports";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<ResponseDTO> UpdateReport(string reportId, int status)
        {
            ResponseDTO response = new();
            try
            {
                var rp = _db.reports.Find(reportId);
                if (rp != null)
                {
                    rp.Status = (ReportsStatus)status;
                    _db.reports.Update(rp);
                    _db.SaveChangesAsync();

                    // Gửi email thông báo sau khi cập nhật thành công
                    string subject = "Your Report Status has been Updated";

                    // Tạo nội dung email với HTML
                    #region htmlMessage
                    string htmlMessage = $@"
                <html>
                    <head>
                        <style>
                            body {{
                                font-family: Arial, sans-serif;
                                background-color: #f4f4f4;
                                color: #333;
                                margin: 0;
                                padding: 20px;
                            }}
                            .email-container {{
                                background-color: #fff;
                                padding: 20px;
                                border-radius: 8px;
                                box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                            }}
                            .email-header {{
                                background-color: #007BFF;
                                color: #fff;
                                padding: 10px;
                                border-radius: 8px 8px 0 0;
                                text-align: center;
                            }}
                            .email-content {{
                                margin-top: 20px;
                            }}
                            .footer {{
                                margin-top: 20px;
                                font-size: 12px;
                                color: #aaa;
                                text-align: center;
                            }}
                            .important-text {{
                                color: #007BFF;
                                font-weight: bold;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='email-container'>
                            <div class='email-header'>
                                <h2>Report Status Update</h2>
                            </div>
                            <div class='email-content'>
                                <p>Dear <span>{rp.UserName}</span>,</p>
                                <p>Your report with ID <span class='important-text'>{reportId}</span> has been updated successfully.</p>
                                <p><strong>New Status:</strong> {rp.Status}</p>
                                <p>G4T Thank you for your patience!</p>
                            </div>
                            <div class='footer'>
                                <p>If you have any questions or need further assistance, please contact our support team.</p>
                                <p>Best regards, <br>Your Support Team</p>
                            </div>
                        </div>
                    </body>
                </html>";
                    #endregion
                    // Gửi email với định dạng HTML
                    SendEmailAsync(rp.Email, subject, htmlMessage);

                    response.Result = rp;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Reports";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        public async Task<ResponseDTO> UpdateUserName(UpdateUserNameModel model)
        {
            ResponseDTO response = new();

            try
            {
                // Tìm tất cả sản phẩm có username trùng với oldusername
                var rps = await _db.reports.Where(p => p.UserName == model.OldUserName).ToListAsync();

                if (rps.Count > 0)
                {
                    // Cập nhật username của tất cả các sản phẩm tìm được
                    foreach (var rp in rps)
                    {
                        rp.UserName = model.NewUserName;
                    }

                    // Lưu thay đổi vào cơ sở dữ liệu
                    await _db.SaveChangesAsync();

                    response.Message = "Update successfully";
                    response.IsSuccess = true;
                }
                else
                {
                    response.Message = "Not found any report";
                    response.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        #region method
        private void SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailstring = _configuration["21"];
            var SenderPassword = _configuration["22"];
            using var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(emailstring, SenderPassword),
                EnableSsl = true
            };

            // Tạo đối tượng MailMessage
            MailMessage mail = new MailMessage(emailstring, email, subject, htmlMessage);
            mail.IsBodyHtml=true;
            smtpClient.Send(mail);
        }

        public async Task<ResponseDTO> GetById(string Id)
        {
            ResponseDTO response = new();
            try
            {
                var report = await _db.reports.FindAsync(Id);
                if (report != null)
                {
                    response.Result = _mapper.Map<Reports>(report);
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any Category";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
        #endregion
    }
}
