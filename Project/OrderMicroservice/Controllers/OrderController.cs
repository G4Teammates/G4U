using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OrderMicroservice.DBContexts;
using OrderMicroservice.DBContexts.Entities;
using OrderMicroservice.Models;

namespace OrderMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly IMapper _mapper;
        public OrderController(OrderDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost]
        public ActionResult ActionResult(Order Order)
        {
            //_context.Add(_mapper.Map<OrderModel, Order>(Order));
            _context.Add(Order);
            _context.SaveChanges();
            return Ok(Order);
        }
        [HttpGet]
        public ActionResult Get1(string id)
        {
            var Orders = _context.Orders.ToList();
            var Order = Orders.Find(u => u.Id == id);
            return Ok(Order);
        }

    }
}
