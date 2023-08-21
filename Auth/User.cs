public record UserDto(string Username, string Password);

public record UserModel
{
	[Required]
	public string UserName { get; set; } = string.Empty;
	[Required]
	public string Password { get; set; } = string.Empty;

}