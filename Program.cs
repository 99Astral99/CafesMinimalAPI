
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CafeDb>(opt =>
{
	opt.UseNpgsql(builder.Configuration.GetConnectionString("NpgsqlConnection"));
});
builder.Services.AddScoped<ICafeRepository, CafeRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
	using var scope = app.Services.CreateScope();
	var db = scope.ServiceProvider.GetRequiredService<CafeDb>();
	db.Database.EnsureCreated();

	app.UseSwagger();
	app.UseSwaggerUI();
}

app.MapGet("/cafes", async (ICafeRepository repository) =>
	await repository.GetCafesAsync())
	.Produces<List<Cafe>>(StatusCodes.Status200OK)
	.WithName("GetAllCafes")
	.WithTags("Getters");

app.MapGet("/cafes/{id}", async (int id, ICafeRepository repository) =>
	await repository.GetCafeAsync(id) is Cafe cafe
	? Results.Ok(cafe)
	: Results.NotFound())
	.Produces<Cafe>(StatusCodes.Status200OK)
	.WithName("GetCafe")
	.WithTags("Getters");

app.MapPost("/cafes", async ([FromBody] Cafe cafe, ICafeRepository repository) =>
	{
		await repository.InsertCafeAsync(cafe);
		await repository.SaveAsync();
		return Results.Created($"/cafes/{cafe.Id}", cafe);
	})
	.Accepts<Cafe>("application/json")
	.Produces<Cafe>(StatusCodes.Status201Created)
	.WithName("CreateCafe")
	.WithTags("Creators");

app.MapPut("/cafes", async ([FromBody] Cafe cafe, ICafeRepository repository) =>
{
	await repository.UpdateCafeAsync(cafe);
	await repository.SaveAsync();
	return Results.NoContent();
})
	.Accepts<Cafe>("application/json")
	.WithName("UpdateCafe")
	.WithTags("Updaters"); ;

app.MapDelete("/cafes/{id}", async (int id, ICafeRepository repository) =>
{
	await repository.DeleteCafeAsync(id);
	await repository.SaveAsync();
	return Results.NoContent();
})
	.WithName("DeleteCafe")
	.WithTags("Deleters");

app.MapGet("/cafes/search/name/{query}",
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
