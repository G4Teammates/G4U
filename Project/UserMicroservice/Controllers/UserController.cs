using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using UserMicroservice.DBContexts;
using UserMicroservice.DBContexts.Entities;
using UserMicroService.Models;

namespace UserMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly IMapper _mapper;
        public UserController(UserDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost]
        public ActionResult ActionResult(UserModel user)
        {
            _context.Add(_mapper.Map<UserModel, User>(user));
            _context.SaveChanges();
            return Ok(user);
        }
        [HttpGet]
        public ActionResult Get1(Guid id)
        {
            var users = _context.Users.ToList();
            var user = users.Find(u => u.Id == id);
            return Ok(user);
        }

    }
}
