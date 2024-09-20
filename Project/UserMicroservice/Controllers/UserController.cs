using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using UserMicroservice.DBContexts;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.Repositories.IRepositories;
using UserMicroService.Models;

namespace UserMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        public UserController(UserDbContext context, IMapper mapper, IUserService userService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
        }

        [HttpPost]
        public ActionResult ActionResult(UserModel user)
        {
            _context.Add(_mapper.Map<UserModel, User>(user));
            _context.SaveChanges();
            return Ok(user);
        }
        [HttpGet("/{id:guid}")]
        public ActionResult Get1(Guid id)
        {
            var users = _context.Users.ToList();
            var user = users.Find(u => u.Id == id);
            return Ok(user);
        }

        [HttpGet("/search")]
        public async Task<ActionResult> FindUsers([FromQuery] string query)
        {
            var users = await _userService.FindUsers(query);
            return Ok(users);
        }


        //[HttpGet("find")]
        //public async Task<ActionResult> SearchAsync([FromQuery]SearchCriteria query)
        //{
        //    var criteria = new SearchCriteriaBuilder()
        //     .SetDisplay(query.DisplayName)
        //     .SetStatus(query.Status)
        //     .SetEmail(query.Email)
        //     .SetPhoneNumber(query.PhoneNumber)
        //     .SetUsername(query.Username)
        //     .Build();

        //    var users = await _userService.FindUsers(criteria);
        //    return Ok(users);
        //}

    }
}
