using Client.Models;
using Client.Models.UserDTO;
using Client.Repositories.Interfaces.User;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Client.Views.Shared.ViewComponents
{
	public class UsersIndexViewComponent(IUserService userService) : ViewComponent
	{
		public readonly IUserService _userService = userService;
		public async Task<IViewComponentResult> InvokeAsync()
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
	}
}
