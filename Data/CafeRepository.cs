public class CafeRepository : ICafeRepository
{
	private readonly CafeDb _context;
	public CafeRepository(CafeDb context)
	{
		_context = context;
	}
	public async Task<Cafe> GetCafeAsync(int cafeId) => await _context.Cafes.FirstOrDefaultAsync(x => x.Id == cafeId);

	public async Task<List<Cafe>> GetCafesAsync() => await _context.Cafes.ToListAsync();

	public async Task<List<Cafe>> GetCafesAsync(string name) =>
		await _context.Cafes.Where(h => h.Name.Contains(name)).ToListAsync();


	public async Task InsertCafeAsync(Cafe cafe) => await _context.Cafes.AddAsync(cafe);

	public async Task UpdateCafeAsync(Cafe cafe)
	{
		var cafeFromDb = await _context.Cafes.FirstOrDefaultAsync(x => x.Id == cafe.Id);
		if (cafeFromDb == null)
			return;
		cafeFromDb.Name = cafe.Name;
		cafeFromDb.Latitude = cafe.Latitude;
		cafeFromDb.Longitude = cafe.Longitude;
	}
	public async Task DeleteCafeAsync(int cafeId)
	{
		var cafeFromDb = await _context.Cafes.FirstOrDefaultAsync(x => x.Id == cafeId);
		if (cafeFromDb == null) return;
		_context.Cafes.Remove(cafeFromDb);
	}

	public async Task SaveAsync() => await _context.SaveChangesAsync();

	private bool _disposed = false;
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				_context.Dispose();
			}
		}
		_disposed = true;
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}