using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderMicroservice.DBContexts;
using OrderMicroservice.DBContexts.Entities;
using OrderMicroservice.Models;
using OrderMicroservice.Models.Message;
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

        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, User")]
        [HttpGet]
        public async Task<ActionResult> GetOrders(int? page, int pageSize)
        {
            try
            {
                int pageNumber = (page ?? 1);
                ResponseModel response = await _orderService.GetAll(pageNumber, pageSize);
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
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpGet("search/{id}")]
        public async Task<ActionResult> GetOrderById(string id)
        {
            try
            {
                ResponseModel response = await _orderService.GetOrderById(id);
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

        [HttpGet("search-payment/{id}")]
        public async Task<ActionResult> GetOrder(string id)
        {
            try
            {
                ResponseModel response = await _orderService.GetOrderByTransaction(id);
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

        [HttpGet("customer/{id}/items")]
        public async Task<ActionResult> GetItemsByCustomerId(string id)
        {
            try
            {
                ResponseModel response = await _orderService.GetItemsByCustomerId(id);
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
        public async Task<ActionResult> Create([FromBody] OrderModel order)
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

        [HttpPut("{id}")]
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
        
        [HttpPut("transid")]
        public async Task<ActionResult> UpdateTrandsId(string orderId, string transId)
        {
            try
            {
                ResponseModel response = await _orderService.UpdateTransId(orderId, transId);
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