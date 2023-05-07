
public class CafeDb : DbContext
{
	public CafeDb(DbContextOptions<CafeDb> options) : base(options) { }

	public DbSet<Cafe> Cafes => Set<Cafe>();
}
