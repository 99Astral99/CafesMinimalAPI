public class UserRepository : IUserRepository
{
	private List<UserDto> _users = new()
	{
		new UserDto("Walther", "password"),
		new UserDto("Kiki", "password"),
		new UserDto("Mike", "password"),
	};
	public UserDto GetUser(UserModel userModel) =>
		_users.FirstOrDefault(u =>
			string.Equals(u.Username, userModel.UserName) && string.Equals(u.Password, userModel.Password)) ??
				throw new Exception();
}