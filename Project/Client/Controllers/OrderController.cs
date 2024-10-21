using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult History()
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
    }
}
