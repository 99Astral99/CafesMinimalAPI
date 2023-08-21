
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CafeDb>(opt =>
{
	opt.UseNpgsql(builder.Configuration.GetConnectionString("NpgsqlConnection"));
});
builder.Services.AddScoped<ICafeRepository, CafeRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(opt => opt.TokenValidationParameters = new()
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,

		ValidIssuer = builder.Configuration["Jwt:Issuer"],
		ValidAudience = builder.Configuration["JwtAudience"],
		IssuerSigningKey = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
	});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setup =>
{
	var jwtSecurityScheme = new OpenApiSecurityScheme
	{
		BearerFormat = "JWT",
		Name = "JWT Authentication",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.Http,
		Scheme = JwtBearerDefaults.AuthenticationScheme,
		Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

		Reference = new OpenApiReference
		{
			Id = JwtBearerDefaults.AuthenticationScheme,
			Type = ReferenceType.SecurityScheme
		}
	};

	setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

	setup.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{ jwtSecurityScheme, Array.Empty<string>() }
	});
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
	using var scope = app.Services.CreateScope();
	var db = scope.ServiceProvider.GetRequiredService<CafeDb>();
	db.Database.EnsureCreated();

	app.UseSwagger();
	app.UseSwaggerUI();
}
app.MapPost("/login", [AllowAnonymous] async (HttpContext context, ITokenService tokenService, [FromBody]UserModel user,
	IUserRepository userRepository) =>
{
	//var user = new UserModel()
	//{
	//	UserName = context.Request.Query["username"],
	//	Password = context.Request.Query["password"]
	//};
	var userDto = userRepository.GetUser(user);
	if (userDto == null)
		return Results.Unauthorized();

	var token = tokenService.BuildToken(builder.Configuration["Jwt:Key"],
		builder.Configuration["Jwt:Issuer"], userDto);

	return Results.Ok(token);
});

app.MapGet("/cafes", [Authorize] async (ICafeRepository repository) =>
	await repository.GetCafesAsync())
	.Produces<List<Cafe>>(StatusCodes.Status200OK)
	.WithName("GetAllCafes")
	.WithTags("Getters");

app.MapGet("/cafes/{id}", [Authorize]async (int id, ICafeRepository repository) =>
	await repository.GetCafeAsync(id) is Cafe cafe
	? Results.Ok(cafe)
	: Results.NotFound())
	.Produces<Cafe>(StatusCodes.Status200OK)
	.WithName("GetCafe")
	.WithTags("Getters");

app.MapPost("/cafes", [Authorize] async ([FromBody] Cafe cafe, ICafeRepository repository) =>
	{
		await repository.InsertCafeAsync(cafe);
		await repository.SaveAsync();
		return Results.Created($"/cafes/{cafe.Id}", cafe);
	})
	.Accepts<Cafe>("application/json")
	.Produces<Cafe>(StatusCodes.Status201Created)
	.WithName("CreateCafe")
	.WithTags("Creators");

app.MapPut("/cafes", [Authorize] async ([FromBody] Cafe cafe, ICafeRepository repository) =>
{
	await repository.UpdateCafeAsync(cafe);
	await repository.SaveAsync();
	return Results.NoContent();
})
	.Accepts<Cafe>("application/json")
	.WithName("UpdateCafe")
	.WithTags("Updaters"); ;

app.MapDelete("/cafes/{id}", [Authorize] async (int id, ICafeRepository repository) =>
{
	await repository.DeleteCafeAsync(id);
	await repository.SaveAsync();
	return Results.NoContent();
})
	.WithName("DeleteCafe")
	.WithTags("Deleters");

app.MapGet("/cafes/search/name/{query}", [Authorize]
async (string query, ICafeRepository repository) =>
		await repository.GetCafesAsync(query) is IEnumerable<Cafe> cafes
		? Results.Ok(cafes)
		: Results.NotFound(Array.Empty<Cafe>()))
	.Produces<List<Cafe>>(StatusCodes.Status200OK)
	.Produces(StatusCodes.Status404NotFound)
	.WithName("SearchCafes")
	.WithTags("Getters")
	.ExcludeFromDescription();


app.UseHttpsRedirection();
app.Run();
