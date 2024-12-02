using Client.Models;
using Client.Repositories.Interfaces.Reports;
using Client.Repositories.Services.Reports;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Client.Controllers
{
    public class ReportController : Controller
    {
        public readonly IReportsService _reportsService;

        public ReportController(IReportsService reportsService)
        {
            _reportsService = reportsService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport(CreateReportsModels model)
        {
            IEnumerable<Claim> claim = HttpContext.User.Claims;
            var UserName = claim.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value!;
            // Kiểm tra nếu UserName không hợp lệ
            if (string.IsNullOrEmpty(UserName))
            {
                TempData["error"] = "Please login first";
                return Json(new { success = false, message = TempData["error"] });
            }

            // Kiểm tra tính hợp lệ của ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                       .SelectMany(v => v.Errors)
                                       .Select(e => e.ErrorMessage);
                TempData["error"] = string.Join(", ", errors);
                return Json(new { success = false, message = TempData["error"] });
            }

            try
            {
                // Gọi service CreateReport
                var response = await _reportsService.CreateReport(model, UserName);

                // Kiểm tra phản hồi từ service
                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Report created successfully";

                    // Nếu cần render PartialView, thực hiện như sau:
                    // var html = await RenderViewAsync("_ReportPartial", model);
                    // return Json(new { success = true, html = html, message = TempData["success"] });

                    return Json(new { success = true, message = TempData["success"] });
                }
                else
                {
                    TempData["error"] = response?.Message ?? "An unknown error occurred.";
                    return Json(new { success = false, message = TempData["error"] });
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                TempData["error"] = $"An error occurred: {ex.Message}";
                return Json(new { success = false, message = TempData["error"] });
            }
        }

    }
}
