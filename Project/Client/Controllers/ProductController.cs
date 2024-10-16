using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Ocelot.Responses;

namespace Client.Controllers
{
    public class ProductController : Controller
    {
        
        public async Task<IActionResult> ProductIndex()
        {
            
            
            return View();
        }
        public IActionResult Product()
        {
            return View();
        }

        public IActionResult ProductDetail()
        {
            return View();
        }

        public IActionResult Collection()
        {
            return View();
        }

    }
}
