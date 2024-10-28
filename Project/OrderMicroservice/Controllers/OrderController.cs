using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OrderMicroservice.DBContexts;
using OrderMicroservice.DBContexts.Entities;
using OrderMicroservice.Models;
using OrderMicroservice.Models.OrderModel;
using OrderMicroservice.Models.PaymentModel;
using OrderMicroservice.Repositories.Interfaces;

namespace OrderMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController(IOrderService orderService) : ControllerBase
    {
        IOrderService _orderService = orderService;

        [HttpGet]
        public async Task<ActionResult> GetOrders()
        {
            try
            {
                ResponseModel response = await _orderService.GetAll();
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

        [HttpGet("{id}")]
        public async Task<ActionResult> GetOrder(string id)
        {
            try
            {
                ResponseModel response = await _orderService.GetOrder(id);
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

        [HttpGet("{id}/items")]
        public async Task<ActionResult> GetItems(string id)
        {
            try
            {
                ResponseModel response = await _orderService.GetOrderItems(id);
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

        [HttpPost]
        public async Task<ActionResult> Create([FromBody]OrderModel order)
        {
            try
            {
                ResponseModel response = await _orderService.Create(order);
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

        [HttpPut]
        public async Task<ActionResult> UpdateStatus(string id, PaymentStatusModel status)
        {
            try
            {
                ResponseModel response = await _orderService.UpdateStatus(id, status);
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
