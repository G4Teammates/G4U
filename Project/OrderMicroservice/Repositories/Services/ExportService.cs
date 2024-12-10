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
namespace OrderMicroservice.Repositories.Services
{
    public class ExportService(IOrderService orderService, IMessage message) : IExportService
    {
        private readonly IOrderService _orderService = orderService;
        private readonly IMessage _message = message;
        private FindUsernameModel _findUserResult;

        public async Task<ResponseModel> Export(DateTime datetime)
        {
            ResponseModel response = new();
            // Tiếp tục xử lý dữ liệu
            ResponseModel responseGroupByProfit = await _orderService.GroupByProfitOrder(datetime);

            if (!responseGroupByProfit.IsSuccess)
            {
                throw new Exception("Can't get group by order to export.");
            }

            var dataGroupBy = (ExportResult)responseGroupByProfit.Result;


            if (dataGroupBy != null)
            {
                int maxRetryAttempts = 3; // Số lần thử lại tối đa
                int retryCount = 0; // Đếm số lần thử lại
                //bool canExport = false; // Khởi tạo biến _canDelete
                bool isCompleted = false; // Biến đánh dấu hoàn thành

                while (retryCount < maxRetryAttempts && !isCompleted)
                {
                    _message.SendingMessageExport(dataGroupBy); // Gửi message

                    var tcs = new TaskCompletionSource<bool>(); // Tạo TaskCompletionSource

                    // Đăng ký sự kiện để gán giá trị
                    _message.OnFindUserModelResponseReceived += (response) =>
                    {
                        //Console.WriteLine($"Received response: {response.CateName}, CanDelete: {response.CanDelete}");
                        _findUserResult = response; // Gán giá trị vào biến
                        // Đánh dấu task đã hoàn thành
                        if (!tcs.Task.IsCompleted)
                        {
                            tcs.SetResult(true);
                            Console.WriteLine("SetResult done");
                        }
                    };

                    // Chờ cho sự kiện được kích hoạt hoặc timeout sau 5 giây
                    var timeoutTask = Task.Delay(5000); // Thời gian timeout là 5 giây
                    var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                    // Nếu timeout xảy ra
                    if (completedTask == timeoutTask)
                    {
                        Console.WriteLine($"Attempt {retryCount + 1} failed due to timeout.");
                        retryCount++; // Tăng số lần thử lại

                        if (retryCount >= maxRetryAttempts)
                        {
                            response.IsSuccess = false;
                            response.Message = $"Exceeded retry count of {retryCount}"; // Trả về lỗi sau khi hết số lần thử lại
                            return response; // Trả về lỗi sau khi hết số lần thử lại
                        }

                        // Nếu chưa đạt giới hạn retry, tiếp tục vòng lặp
                        continue;
                    }

                    // Nếu có phản hồi, thoát khỏi vòng lặp
                    isCompleted = true;

                    // Xử lý kết quả khi có phản hồi từ RabbitMQ
                    if (_findUserResult != null)
                    {
                        response.IsSuccess = true;
                        response.Message = "Prepare successfully";
                    }
                    else
                    {
                        response.IsSuccess = true;
                        response.Message = "Prepare failed";
                        return response;
                    }
                }
            }
            else
            {
                response.IsSuccess = false;
                response.Message = "Not data to export";
                return response;
            }

            FindUsernameModel data = _findUserResult;

            response = await FormatExcel(data);
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
