using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using CommentMicroservice.DBContexts;
using CommentMicroservice.DBContexts.Entities;

namespace CommentMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly CommentDbContext _context;
        private readonly IMapper _mapper;
        public CommentController(CommentDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost]
        public ActionResult ActionResult(Comment Comment)
        {
            //_context.Add(_mapper.Map<CommentModel, Comment>(Comment));
            _context.Add(_mapper.Map<Comment>(Comment));
            _context.SaveChanges();
            return Ok(Comment);
        }
        [HttpGet]
        public ActionResult Get1(Guid id)
        {
            var Comments = _context.Comments.ToList();
            var Comment = Comments.Find(u => u.Id == id);
            return Ok(Comment);
        }

    }
}
