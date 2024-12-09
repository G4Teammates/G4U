using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using OrderMicroservice.Models;
using OrderMicroservice.Repositories.Interfaces;

namespace OrderMicroservice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExportController(IExportService exportService) : ControllerBase
    {
        private readonly IExportService _exportService = exportService;

        [HttpPost]
        public async Task<IActionResult> Export(DateTime datetime)
        {
            try
            {
                ResponseModel response = await _exportService.Export(datetime);
                if (response.IsSuccess)
                {
                    FileStreamResult file = (FileStreamResult)response.Result;
                    return file;
                }
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                // Trả về lỗi 500 cho các lỗi chưa dự đoán
                return StatusCode(500, new { message = "An unexpected error occurred. Detail: " + ex.Message });
            }
        }

    }
}
