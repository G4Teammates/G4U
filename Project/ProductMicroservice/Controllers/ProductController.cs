using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ProductMicroservice.DBContexts;
using ProductMicroservice.DBContexts.Entities;
using ProductMicroservice.Models;

namespace ProductMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductDbContext _context;
        private readonly IMapper _mapper;
        public ProductController(ProductDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost]
        public ActionResult ActionResult(ProductModel Product)
        {
            _context.Add(_mapper.Map<ProductModel, Products>(Product));
            _context.SaveChanges();
            return Ok(Product);
        }
        [HttpGet]
        public ActionResult Get1(string id)
        {
            var Products = _context.Products.ToList();
            var Product = Products.Find(u => u.Id == id);
            return Ok(Product);
        }

    }
}
