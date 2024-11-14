using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StatisticalMicroservice.Model.DTO;
using StatisticalMicroservice.Models.DTO;
using StatisticalMicroservice.Repostories;
using X.PagedList.Extensions;

namespace StatisticalMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticalController : ControllerBase
    {
        private ResponseDTO _responseDTO;
        private readonly IRepo _repo;
        public StatisticalController(IRepo repo)
        {
            _responseDTO = new ResponseDTO();
            _repo = repo;
        }
        //[Authorize(Roles = "Admin")]
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
                _responseDTO.Message = Pros.Message;
                return BadRequest(_responseDTO.Message);
            }
            catch (Exception ex)
            {
                _responseDTO.IsSuccess = false;
                _responseDTO.Message = "An error occurred while getting the Statisticals: " + ex.Message;
                return StatusCode(500, _responseDTO); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateStas( CreateStatisticalModel model)
        {
            try
            {
                var stas = await _repo.CreateStastistical(model);
                if (stas.IsSuccess)
                {
                    _responseDTO.Result = stas.Result;
                    return Ok(_responseDTO);
                }
                _responseDTO.Message = stas.Message;
                return BadRequest(_responseDTO.Message);
            }
            catch (Exception ex)
            {
                _responseDTO.IsSuccess = false;
                _responseDTO.Message = "An error occurred while Creating the Stastistical: " + ex.Message;
                return StatusCode(500, _responseDTO); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }
    }
}
