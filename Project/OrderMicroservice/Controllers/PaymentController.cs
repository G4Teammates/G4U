using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderMicroservice.Models;
using OrderMicroservice.Models.PaymentModel;
using OrderMicroservice.Repositories.Interfaces;
using System.Security.Cryptography;
using System.Text;

using Net.payOS;
using OrderMicroservice.Models.OrderModel;
using OrderMicroservice.Models.PaymentModel.MoMo;
using OrderMicroservice.Models.PaymentModel.PayOsModel;

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
        public async Task<ActionResult> MoMoPayment([FromBody]MoMoRequestFromClient requestClient)
        {
            try
            {
                ResponseModel response = await _paymentService.MoMoPayment(requestClient);
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
        public async Task<ActionResult> VietQRPayment(VietQRRequest request)
        {
            try
            {
                ResponseModel response = await _paymentService.VierQRPayment(request);
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
        [HttpPost("ipn/momo")]
        public async Task<ActionResult> IpnMoMo(MoMoIPNResquest request)
        {
            try
            {
                ResponseModel response = await _paymentService.IpnMoMo(request);
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
        [HttpPost("paid")]
        public async Task<ActionResult> Paid(PaidModel request)
        {
            try
            {
                ResponseModel response = await _paymentService.Paid(request);
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
        [HttpPost("send-notification")]
        public async Task<ActionResult> SendNotification([FromBody]SendMailModel model)
        {
            try
            {
                ResponseModel response = await _paymentService.SendNotification(model);
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