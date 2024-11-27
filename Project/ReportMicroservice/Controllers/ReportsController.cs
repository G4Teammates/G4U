using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReportMicroservice.Models;
using ReportMicroservice.Repostories;

namespace ReportMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private ResponseDTO _responseDTO;
        private readonly IRepo _repo;
        public ReportsController(IRepo repo)
        {
            _responseDTO = new ResponseDTO();
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int? page, int pageSize)
        {
            try
            {
                int pageNumber = (page ?? 1);
                var Pros = await _repo.GetAll(pageNumber, pageSize);
                if (Pros.IsSuccess)
                {
                    _responseDTO.Result = Pros.Result;
                    return Ok(_responseDTO);
                }
                _responseDTO.IsSuccess = false;
                _responseDTO.Message = Pros.Message;
                return BadRequest(_responseDTO);
            }
            catch (Exception ex)
            {
                _responseDTO.IsSuccess = false;
                _responseDTO.Message = "An error occurred while getting the Reports: " + ex.Message;
                return StatusCode(500, _responseDTO); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }

        [HttpPost("userName/{UserName}")]
        public async Task<IActionResult> CreateComment([FromForm] CreateReportsModels model, string UserName)
        {
            try
            {
                var Comm = await _repo.CreateReport(model,UserName);
                if (Comm.IsSuccess)
                {
                    _responseDTO.Result = Comm.Result;
                    return Ok(_responseDTO);
                }
                _responseDTO.IsSuccess = false;
                _responseDTO.Message = Comm.Message;
                return BadRequest(_responseDTO);
            }
            catch (Exception ex)
            {
                _responseDTO.IsSuccess = false;
                _responseDTO.Message = "An error occurred while getting the report: " + ex.Message;
                return StatusCode(500, _responseDTO); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(string reportId, int status)
        {
            try
            {
                var Pros = await _repo.UpdateReport(reportId, status);
                if (Pros.IsSuccess)
                {
                    _responseDTO.Result = Pros.Result;
                    return Ok(_responseDTO);
                }
                _responseDTO.IsSuccess = false;
                _responseDTO.Message = Pros.Message;
                return BadRequest(_responseDTO);
            }
            catch (Exception ex)
            {
                _responseDTO.IsSuccess = false;
                _responseDTO.Message = "An error occurred while getting the Reports: " + ex.Message;
                return StatusCode(500, _responseDTO); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }
    }
}
