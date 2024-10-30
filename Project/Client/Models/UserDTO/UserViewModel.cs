namespace Client.Models.UserDTO
{
	public class UserViewModel
	{
		public CreateUser? CreateUser{get; set;}
		public UpdateUser? UpdateUser { get; set; }
		public ICollection<UsersDTO>? Users { get; set; }

		public int pageNumber { get; set; }
		public int pageSize { get; set; }
		public int totalItem { get; set; }
		public int pageCount { get; set; }
	}
}
