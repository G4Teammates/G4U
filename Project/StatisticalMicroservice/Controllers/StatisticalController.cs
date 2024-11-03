using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet]
        public IActionResult GetAll(int? page, int pageSize)
        {
            try
            {
                int pageNumber = (page ?? 1);
                var Products = _repo.Statisticals;
                _responseDTO.Result = Products.ToPagedList(pageNumber, pageSize);
                return Ok(_responseDTO);
            }
            catch (Exception ex)
            {
                _responseDTO.IsSuccess = false;
                _responseDTO.Message = "An error occurred while getting the Statisticals: " + ex.Message;
                return StatusCode(500, _responseDTO); // Trả về mã lỗi 500 với thông báo lỗi chi tiết
            }
        }
    }
}
