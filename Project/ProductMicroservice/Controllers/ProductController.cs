using Microsoft.AspNetCore.Mvc;
using ProductMicroservice.Models;

namespace ProductMicroservice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        public ProductController()
        {
        }

        [HttpPost]
        public ActionResult CreateProduct([FromBody] ProductModel productInput)
        {
            return Ok(productInput);
        }
    }
}
