using OfficeOpenXml;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using OfficeOpenXml.Style;
using Microsoft.AspNetCore.Mvc;
using System;
using UserMicroservice.Models.UserManagerModel;
using UserMicroservice.Repositories.Interfaces;
using UserMicroservice.Models.Message;
using UserMicroservice.Models;
namespace UserMicroservice.Repositories.Services
{
    public class ExportService(IUserService userService, IMessage message) : IExportService
    {
        private readonly IUserService _userService = userService;
        private readonly IMessage _message = message;
        public async Task<FileStreamResult> Export(ExportResult model)
        {

            ResponseModel response = await _userService.GetUserByListUsername(model);

            if (!response.IsSuccess == null)
            {
                response.IsSuccess = false;
                response.Message = "Can't get data to export";
            }

            var dataExcel = (FindUsernameModel)response.Result;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("GroupedData");

                // Thêm tiêu đề cột
                worksheet.Cells[1, 1].Value = "Id";
                worksheet.Cells[1, 2].Value = "Publisher Name";
                worksheet.Cells[1, 3].Value = "Bank Name";
                worksheet.Cells[1, 4].Value = "Bank Account";
                worksheet.Cells[1, 5].Value = "Email";
                worksheet.Cells[1, 6].Value = "Phone";
                worksheet.Cells[1, 7].Value = "Profit of Month";
                worksheet.Cells[1, 8].Value = "Original Price of Month";

                // Định dạng tiêu đề
                using (var range = worksheet.Cells[1, 1, 1, 8])
                {
                    range.Style.Font.Bold = true;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
                // Ghi dữ liệu vào các dòng
                // Thêm dữ liệu vào từng dòng
                List<ExportProfitModel> usersExport = (List<ExportProfitModel>)dataExcel.UsersExport;
                for (int i = 0; i < usersExport.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = usersExport[i].Id;                    // Cột Id
                    worksheet.Cells[i + 2, 2].Value = usersExport[i].PublisherName;         // Cột Publisher name
                    worksheet.Cells[i + 2, 3].Value = usersExport[i].BankName;              // Cột Bank Name
                    worksheet.Cells[i + 2, 4].Value = usersExport[i].BankAccount;           // Cột Bank Account
                    worksheet.Cells[i + 2, 5].Value = usersExport[i].Email;                 // Cột Email
                    worksheet.Cells[i + 2, 6].Value = usersExport[i].PhoneNumber;                 // Cột Phone
                    worksheet.Cells[i + 2, 7].Value = usersExport[i].ProfitOfMonth;         // Cột Profit of Month
                    worksheet.Cells[i + 2, 8].Value = usersExport[i].OriginalPriceOfMonth;  // Cột Original Price of Month
                }


                // Tự động điều chỉnh kích thước cột
                worksheet.Cells.AutoFitColumns();

                // Lưu file vào MemoryStream
                var stream = new MemoryStream();
                await package.SaveAsAsync(stream);
                stream.Position = 0;

                var fileName = "GroupedData.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                // Trả về FileStreamResult
                return new FileStreamResult(stream, contentType)
                {
                    FileDownloadName = fileName
                };
            }
        }

    }
}
