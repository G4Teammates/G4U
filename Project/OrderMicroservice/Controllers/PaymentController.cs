using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderMicroservice.Models;
using OrderMicroservice.Models.PaymentModel;
using OrderMicroservice.Repositories.Interfaces;
using System.Security.Cryptography;
using System.Text;

using Net.payOS;

namespace OrderMicroservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost("momo")]
        public async Task<ActionResult> MoMoPayment([FromBody] string id, long amount)
        {
            try
            {
                ResponseModel response = await _paymentService.MoMoPayment(id, amount);
                if (response.IsSuccess)
                    return Ok(response);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                // Trả về lỗi 500 cho các lỗi chưa dự đoán
                return StatusCode(500, new { message = "An unexpected error occurred. Detail" + ex.Message });
            }
        }
        [HttpPost("vietqr")]
        public async Task<ActionResult> VietQRPayment([FromBody] string id, int amount, string productName, int quantity)
        {
            try
            {
                ResponseModel response = await _paymentService.VierQRPayment(id, amount, productName, quantity);
                if (response.IsSuccess)
                    return Ok(response);
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                // Trả về lỗi 500 cho các lỗi chưa dự đoán
                return StatusCode(500, new { message = "An unexpected error occurred. Detail" + ex.Message });
            }
        }
    }
}