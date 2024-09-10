using Microsoft.AspNetCore.Mvc;
using UserMicroservice.DBContexts.Entities;
using UserMicroService.Models;

namespace UserMicroService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {

        public UserController()
        {

        }

        [HttpPost]
        public ActionResult ActionResult(UserInput user)
        {

            return Ok();
        }

    }
}
