using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using CategoryMicroservice.DBContexts;
using CategoryMicroservice.DBContexts.Entities;

namespace CategoryMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryDbContext _context;
        private readonly IMapper _mapper;
        public CategoryController(CategoryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost]
        public ActionResult ActionResult(Category Category)
        {
            //_context.Add(_mapper.Map<CategoryModel, Category>(Category));
            _context.Add(Category);
            _context.SaveChanges();
            return Ok(Category);
        }
        [HttpGet]
        public ActionResult Get1(Guid id)
        {
            var Categorys = _context.Categories.ToList();
            var Category = Categorys.Find(u => u.Id == id);
            return Ok(Category);
        }

    }
}
