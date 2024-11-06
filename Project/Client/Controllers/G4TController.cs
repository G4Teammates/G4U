using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{
    public class G4TController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Blog()
        {
            return View();
        }

        public IActionResult ContactUs()
        {
            return View();  
        }
        public IActionResult Help()
        {
            return View();
        }
    }
}
