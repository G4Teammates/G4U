using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using LinkMicroservice.DBContexts;
using LinkMicroservice.DBContexts.Entities;

namespace LinkMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LinkController : ControllerBase
    {
        private readonly LinkDbContext _context;
        private readonly IMapper _mapper;
        public LinkController(LinkDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost]
        public ActionResult ActionResult(Link Link)
        {
            //_context.Add(_mapper.Map<LinkModel, Link>(Link));
            _context.Add(Link);
            _context.SaveChanges();
            return Ok(Link);
        }
        [HttpGet]
        public ActionResult Get1(Guid id)
        {
            var Links = _context.Links.ToList();
            var Link = Links.Find(u => u.Id == id);
            return Ok(Link);
        }

    }
}
