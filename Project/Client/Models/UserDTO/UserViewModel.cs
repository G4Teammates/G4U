namespace Client.Models.UserDTO
{
	public class UserViewModel
	{
		public CreateUser? CreateUser{get; set;}
		public UpdateUser? UpdateUser { get; set; }
		public ICollection<UsersDTO>? Users { get; set; }

	}
}
