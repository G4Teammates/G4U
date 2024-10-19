using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Cart()
        {
            return View();
        }

        public IActionResult PaymentSuccess()
        {
            return View();
        }

        public IActionResult PaymentFailure()
        {
            return View();
        }
    }
}
