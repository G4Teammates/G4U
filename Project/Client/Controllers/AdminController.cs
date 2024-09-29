using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AdminDashboard()
        {
            return View();
        }

        public IActionResult UsersManager()
        {
            return View();
        }

		public IActionResult ProductsManager()
		{
			return View();
		}

        public IActionResult OrdersManager()
        {
            return View();
        }
    }
}
