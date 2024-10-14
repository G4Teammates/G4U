using Client.Models;
using Client.Models.UserDTO;
using Client.Repositories.Interfaces.User;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Client.Controllers
{
    public class AdminController (IUserService userService) : Controller
    {
	
		public readonly IUserService _userService = userService;
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

		public async Task<IActionResult> UserUpdate(string id)
		{
            ResponseModel? response = await _userService.GetUserAsync(id);

            if (response != null && response.IsSuccess)
            {
                UpdateUser? user = JsonConvert.DeserializeObject<UpdateUser>(Convert.ToString(response.Result));

                // Trả về model UsersDTO để sử dụng trong View
                return View(user);
            }
            else
            {
                TempData["error"] = response?.Message;
            }
            return NotFound();
        }

		[HttpPost]
		public async Task<IActionResult> UserUpdate(UpdateUser user)
		{
            if (ModelState.IsValid)
            {
                var updateUser = new UpdateUser
                {
					Id = user.Id,
                    Username = user.Username,
                    PhoneNumber = user.PhoneNumber,
                    DisplayName = user.DisplayName
                    // Nếu bạn có thêm thuộc tính, hãy thêm vào đây
                };

                ResponseModel? response = await _userService.UpdateUser(updateUser);

                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "User updated successfully";
                    return RedirectToAction(nameof(UsersManager));
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            }

            // Nếu ModelState không hợp lệ, trả về lại model để hiển thị lỗi
            return View(user);

        }


        public IActionResult ProductsManager()
		{
			return View();
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
