using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
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
