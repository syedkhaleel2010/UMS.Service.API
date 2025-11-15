using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UMS.Core.Entities;

namespace UMS.Core.Interfaces
{
    public interface IRepository<T> where T : EntityBase
    {
        void Add(params T[] entity);

        void AddOrUpdate(params T[] entities);

        void AddOrUpdate(List<T> entities);

        void Remove(params T[] entities);

        void Remove(List<T> entities);

        void RemoveByKey(params string[] Keys);

        void RemoveByKey(List<string> Keys);

        int Delete(params T[] entities);

        int Delete(List<T> entities);

        int DeleteByKey(params string[] Keys);

        int DeleteByKey(List<string> Keys);

        Task<int> DeleteAsync(params T[] entities);

        Task<int> DeleteAsync(List<T> entities);

        Task<int> DeleteByKeyAsync(params string[] Keys);

        Task<int> DeleteByKeyAsync(List<string> Keys);

        int Save(params T[] entities);

        int Save(List<T> entities);

        Task<int> SaveAsync(params T[] entities);

        Task<int> SaveAsync(List<T> entities);

        int CommitChanges();

        Task<int> CommitChangesAsync();

        Task ReloadAsync(params T[] model);

        Task ReloadAsync(List<T> entities);

        bool HasData();

        bool HasData(Expression<Func<T, bool>> filter);

        Task<bool> HasDataAsync();

        Task<bool> HasDataAsync(Expression<Func<T, bool>> filter);

        int Count();

        int Count(Expression<Func<T, bool>> filter);

        Task<int> CountAsync();

        Task<int> CountAsync(Expression<Func<T, bool>> filter);

        long CountLong();

        long CountLong(Expression<Func<T, bool>> filter);

        Task<long> CountLongAsync();

        Task<long> CountLongAsync(Expression<Func<T, bool>> filter);

        T Max();

        Task<T> MaxAsync();

        T GetByKey(string key);

        Task<T> GetByKeyAsync(string key);

        T Get(Expression<Func<T, bool>> filter, params string[] includeProperties);

        Task<T> GetAsync(Expression<Func<T, bool>> filter, params string[] includeProperties);

        T GetReadOnly(Expression<Func<T, bool>> filter, params string[] includeProperties);

        Task<T> GetReadOnlyAsync(Expression<Func<T, bool>> filter, params string[] includeProperties);

        IQueryable<T> GetAll(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, int? skip = null, int? take = null, params string[] includePathTypes);

        IQueryable<T> GetAllReadOnly(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, int? skip = null, int? take = null, params string[] includePathTypes);
    }
}
