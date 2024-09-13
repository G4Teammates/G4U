using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using CollectionMicroservice.DBContexts;
using CollectionMicroservice.DBContexts.Entities;

namespace CollectionMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CollectionController : ControllerBase
    {
        private readonly CollectionDbContext _context;
        private readonly IMapper _mapper;
        public CollectionController(CollectionDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost]
        public ActionResult ActionResult(Collection Collection)
        {
            //_context.Add(_mapper.Map<CollectionModel, Collection>(Collection));
            _context.Add(Collection);
            _context.SaveChanges();
            return Ok(Collection);
        }
        [HttpGet]
        public ActionResult Get1(Guid id)
        {
            var Collections = _context.Collections.ToList();
            var Collection = Collections.Find(u => u.Id == id);
            return Ok(Collection);
        }

    }
}
