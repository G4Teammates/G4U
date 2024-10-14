using Client.Models;
using Client.Models.Product_Model;
using Client.Models.UserDTO;
using Client.Repositories.Interfaces.ProductInterface;
using Client.Repositories.Interfaces.User;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Client.Controllers
{
    public class AdminController (IUserService userService, IRepoProduct productService) : Controller
    {
	
		public readonly IUserService _userService = userService;
		public readonly IRepoProduct _productService = productService;
		public IActionResult Index()
        {
            return View();
        }

		public IActionResult AdminDashboard()
        {
			return View();
        }

		[HttpGet]
		public async Task<IActionResult> UsersManager()
        {
			List<UsersDTO?> list = new();
			ResponseModel? response = await _userService.GetAllUserAsync();

			if (response != null && response.IsSuccess)
			{

				list = JsonConvert.DeserializeObject<List<UsersDTO>>(Convert.ToString(response.Result.ToString()));

			}
			else
			{
				TempData["error"] = response?.Message;
			}

			return View(list);
		}


		[HttpPost]
		public async Task<IActionResult> UserCreate(UsersDTO user)
		{
			if (ModelState.IsValid)
			{
				ResponseModel? response = await _userService.CreateUserAsync(user);

				if (response != null && response.IsSuccess)
				{
					TempData["success"] = "Product created successfully";
					return RedirectToAction(nameof(UsersManager));
				}
				else
				{
					TempData["error"] = response?.Message;
				}

			}
			return View(user);
		}

		public async Task<IActionResult> UsersDelete(string id)
		{
			ResponseModel? response = await _userService.GetUserAsync(id);

			if (response != null && response.IsSuccess)
			{
				UsersDTO? model = JsonConvert.DeserializeObject<UsersDTO>(Convert.ToString(response.Result));
				return View(model);
			}
			else
			{
				TempData["error"] = response?.Message;
			}
			return NotFound();
		}

		[HttpPost]
		public async Task<IActionResult> UsersDelete(UsersDTO user)
		{
			ResponseModel? response = await _userService.DeleteUser(user.Id);

			if (response != null && response.IsSuccess)
			{
				TempData["success"] = "User deleted successfully";
				return RedirectToAction(nameof(UsersManager));
			}
			else
			{
				TempData["error"] = response?.Message;
			}
			return BadRequest();
		}

		public async Task<IActionResult> ProductsManager()
		{
            ResponseModel? response = await _productService.GetAllProductAsync();

            if (response != null && response.IsSuccess)
            {
                ProductModel? model = JsonConvert.DeserializeObject<ProductModel>(Convert.ToString(response.Result));
                return View(model);
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return NotFound();
        }

        public IActionResult OrdersManager()
        {
            return View();
        }

		public IActionResult CategoriesManager()
		{
			return View();
		}

		public IActionResult CensorshipManager()
		{
			return View();
		}
	}
}
