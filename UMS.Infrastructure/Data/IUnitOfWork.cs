namespace UMS.Infrastructure.Data
{
        public interface IUnitOfWork
        {
            int Commit();
            Task<int> CommitAsync();
        }
}