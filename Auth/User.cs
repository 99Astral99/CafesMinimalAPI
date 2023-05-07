public record UserDto(string email, string password);

public record UserModel
{
	public string UserName { get; set; } = string.Empty;
	public string Password { get; set; }

}