
public interface ICafeRepository : IDisposable
{
	Task<List<Cafe>> GetCafesAsync();
	Task<List<Cafe>> GetCafesAsync(string name);
	Task<Cafe> GetCafeAsync(int cafeId);
	Task InsertCafeAsync(Cafe cafe);
	Task UpdateCafeAsync(Cafe cafe);
	Task DeleteCafeAsync(int cafeId);
	Task SaveAsync();
}