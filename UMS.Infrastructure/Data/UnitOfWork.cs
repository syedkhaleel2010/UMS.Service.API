
namespace UMS.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly UMSDbContext dbContext;

        public UnitOfWork(UMSDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        public int AffectedRows { get; private set; }

        public int Commit()
        {
            AffectedRows = dbContext.SaveChanges();
            return AffectedRows;
        }

        public async Task<int> CommitAsync()
        {
            try
            {
                AffectedRows = await dbContext.SaveChangesAsync();
                return AffectedRows;
            }
            catch
            {
                throw;
            }
        }
    }
}