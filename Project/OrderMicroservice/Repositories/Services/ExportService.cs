using OfficeOpenXml;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using OfficeOpenXml.Style;
using Microsoft.AspNetCore.Mvc;
using System;
using OrderMicroservice.Repositories.Interfaces;
using OrderMicroservice.Models;
using OrderMicroservice.Models.UserModel;
using DocumentFormat.OpenXml.Wordprocessing;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Azure.Core;
using RabbitMQ.Client;
namespace OrderMicroservice.Repositories.Services
{
    public class ExportService(IOrderService orderService, IMessage message, IHttpContextAccessor httpContext) : IExportService
    {
        private readonly IOrderService _orderService = orderService;
        private readonly IMessage _message = message;
        private FindUsernameModel _findUserResult;
        public IHttpContextAccessor _httpContext = httpContext;
        public async Task<ResponseModel> Export(DateTime datetime)
        {
            ResponseModel response = new();
            ResponseModel responseGroupByProfit = await _orderService.GroupByProfitOrder(datetime);

            if (!responseGroupByProfit.IsSuccess)
            {
                throw new Exception("Can't get group by order to export.");
            }

            var dataGroupBy = (ExportResult)responseGroupByProfit.Result;

            if (dataGroupBy == null)
            {
                response.IsSuccess = false;
                response.Message = "No data to export.";
                return response;
            }

            int maxRetryAttempts = 3;
            int retryCount = 0;
            bool isCompleted = false;

            while (retryCount < maxRetryAttempts && !isCompleted)
            {
                _message.SendingMessage2(dataGroupBy, "Export", "findUser_for_export", "findUser_for_export", ExchangeType.Direct, true, false, false, false);

                var tcs = new TaskCompletionSource<bool>();

                _message.OnFindUserModelResponseReceived += (responseData) =>
                {
                    _findUserResult = responseData;
                    if (!tcs.Task.IsCompleted)
                    {
                        tcs.SetResult(true);
                    }
                };

                var timeoutTask = Task.Delay(5000);
                var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    retryCount++;
                    if (retryCount >= maxRetryAttempts)
                    {
                        response.IsSuccess = false;
                        response.Message = $"Exceeded retry count of {retryCount}.";
                        return response;
                    }
                    continue;
                }

                isCompleted = true;
                if (_findUserResult == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Prepare failed.";
                    return response;
                }
            }

            response = await FormatExcel(_findUserResult);
            if (!response.IsSuccess)
            {
                return response;
            }

            // Lưu file tạm và tạo link tải
            var fileStream = (FileStreamResult)response.Result;
            var rootPath = Directory.GetCurrentDirectory();
            var tempFolder = Path.Combine(rootPath, "wwwroot", "temp");

            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            var tempFilePath = Path.Combine(tempFolder, $"ExportedData_{Guid.NewGuid()}.xlsx");

            using (var file = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.FileStream.CopyToAsync(file);
            }

            string fileUrl = $"{_httpContext.HttpContext.Request.Scheme}://{_httpContext.HttpContext.Request.Host}/temp/{Path.GetFileName(tempFilePath)}";

            // Cập nhật kết quả trả về
            response.IsSuccess = true;
            response.Message = "Export completed.";
            response.Result = fileUrl;

            return response;
        }


        public async Task<ResponseModel> FormatExcel(FindUsernameModel data)
        {
            ResponseModel response = new();
            try
            {
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
                    List<ExportUserModel> usersExport = (List<ExportUserModel>)data.UsersExport;
                    for (int i = 0; i < usersExport.Count; i++)
                    {
                        worksheet.Cells[i + 2, 1].Value = usersExport[i].Id;
                        worksheet.Cells[i + 2, 2].Value = usersExport[i].PublisherName;
                        worksheet.Cells[i + 2, 3].Value = usersExport[i].BankName;
                        worksheet.Cells[i + 2, 4].Value = usersExport[i].BankAccount;
                        worksheet.Cells[i + 2, 5].Value = usersExport[i].Email;
                        worksheet.Cells[i + 2, 6].Value = usersExport[i].PhoneNumber;
                        worksheet.Cells[i + 2, 7].Value = usersExport[i].ProfitOfMonth;
                        worksheet.Cells[i + 2, 8].Value = usersExport[i].OriginalPriceOfMonth;
                    }

                    // Tự động điều chỉnh kích thước cột
                    worksheet.Cells.AutoFitColumns();

                    // Lưu file vào MemoryStream
                    var stream = new MemoryStream();
                    await package.SaveAsAsync(stream);
                    stream.Position = 0;

                    var fileName = "GroupedData.xlsx";
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                    var file = new FileStreamResult(stream, contentType)
                    {
                        FileDownloadName = fileName
                    };

                    response.Result = file;
                    response.Message = "Export successfully";
                    response.IsSuccess = true;
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error : {ex.Message}";
            }
            return response;
           
        }
    }
}
