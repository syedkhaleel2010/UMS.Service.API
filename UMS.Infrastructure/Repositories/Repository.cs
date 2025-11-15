using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UMS.Core.Entities;
using UMS.Core.Interfaces;

namespace UMS.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : EntityBase
    {
        private readonly DbSet<T> dbSet;

        private readonly DbContext dbContext;

        public Repository(DbContext _dbContext)
        {
            dbContext = _dbContext;
            dbSet = dbContext.Set<T>();
        }

        public virtual void Add(params T[] entities)
        {
            dbSet.AddRange(entities);
        }

        public virtual void AddOrUpdate(params T[] entities)
        {
            save(entities);
        }

        public virtual void AddOrUpdate(List<T> entities)
        {
            save(entities.ToArray());
        }

        public virtual void Remove(params T[] entities)
        {
            delete(entities);
        }

        public virtual void Remove(List<T> entities)
        {
            delete(entities.ToArray());
        }

        public virtual void RemoveByKey(params string[] Keys)
        {
            delete(Keys.Select((string d) => GetByKey(d)).ToArray());
        }

        public virtual void RemoveByKey(List<string> Keys)
        {
            delete(Keys.Select((string d) => GetByKey(d)).ToArray());
        }

        public virtual int Delete(params T[] entities)
        {
            delete(entities);
            return CommitChanges();
        }

        public virtual int Delete(List<T> entities)
        {
            return Delete(entities.ToArray());
        }

        public virtual int DeleteByKey(params string[] Keys)
        {
            RemoveByKey(Keys);
            return CommitChanges();
        }

        public virtual int DeleteByKey(List<string> Keys)
        {
            return DeleteByKey(Keys.ToArray());
        }

        public virtual async Task<int> DeleteAsync(params T[] entities)
        {
            delete(entities);
            return await CommitChangesAsync();
        }

        public virtual async Task<int> DeleteAsync(List<T> entities)
        {
            return await DeleteAsync(entities.ToArray());
        }

        public virtual async Task<int> DeleteByKeyAsync(params string[] Keys)
        {
            RemoveByKey(Keys);
            return await CommitChangesAsync();
        }

        public virtual async Task<int> DeleteByKeyAsync(List<string> Keys)
        {
            return await DeleteByKeyAsync(Keys.ToArray());
        }

        public virtual int Save(params T[] entities)
        {
            save(entities);
            return CommitChanges();
        }

        public virtual int Save(List<T> entities)
        {
            return Save(entities.ToArray());
        }

        public virtual async Task<int> SaveAsync(params T[] entities)
        {
            save(entities);
            return await CommitChangesAsync();
        }

        public virtual async Task<int> SaveAsync(List<T> entities)
        {
            return await SaveAsync(entities.ToArray());
        }

        public virtual int CommitChanges()
        {
            return dbContext.SaveChanges();
        }

        public virtual async Task<int> CommitChangesAsync()
        {
            return await dbContext.SaveChangesAsync();
        }

        public virtual async Task ReloadAsync(params T[] entities)
        {
            foreach (T entity in entities)
            {
                await dbContext.Entry(entity).ReloadAsync();
            }
        }

        public virtual async Task ReloadAsync(List<T> entities)
        {
            await ReloadAsync(entities.ToArray());
        }

        public virtual bool HasData()
        {
            return dbSet.Any();
        }

        public virtual bool HasData(Expression<Func<T, bool>> filter)
        {
            return dbSet.Any(filter);
        }

        public virtual async Task<bool> HasDataAsync()
        {
            return await dbSet.AnyAsync();
        }

        public virtual async Task<bool> HasDataAsync(Expression<Func<T, bool>> filter)
        {
            return await dbSet.AnyAsync(filter);
        }

        public virtual int Count()
        {
            return dbSet.Count();
        }

        public virtual int Count(Expression<Func<T, bool>> filter)
        {
            return dbSet.Count(filter);
        }

        public virtual async Task<int> CountAsync()
        {
            return await dbSet.CountAsync();
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> filter)
        {
            return await dbSet.CountAsync(filter);
        }

        public virtual long CountLong()
        {
            return dbSet.LongCount();
        }

        public virtual long CountLong(Expression<Func<T, bool>> filter)
        {
            return dbSet.LongCount(filter);
        }

        public virtual async Task<long> CountLongAsync()
        {
            return await dbSet.LongCountAsync();
        }

        public virtual async Task<long> CountLongAsync(Expression<Func<T, bool>> filter)
        {
            return await dbSet.LongCountAsync(filter);
        }

        public virtual T Max()
        {
            return dbSet.Max();
        }

        public virtual async Task<T> MaxAsync()
        {
            return await dbSet.MaxAsync();
        }

        public virtual T GetByKey(string key)
        {
            return dbSet.Find(key);
        }

        public virtual async Task<T> GetByKeyAsync(string key)
        {
            return await dbContext.Set<T>().FindAsync(new object[1] { key });
        }

        public virtual T Get(Expression<Func<T, bool>> filter, params string[] includeProperties)
        {
            return GetAll(filter, null, null, null, includeProperties).FirstOrDefault();
        }

        public virtual async Task<T> GetAsync(Expression<Func<T, bool>> filter, params string[] includeProperties)
        {
            return await GetAll(filter, null, null, null, includeProperties).FirstOrDefaultAsync();
        }

        public virtual T GetReadOnly(Expression<Func<T, bool>> filter, params string[] includeProperties)
        {
            return GetAllReadOnly(filter, null, null, null, includeProperties).FirstOrDefault();
        }

        public virtual async Task<T> GetReadOnlyAsync(Expression<Func<T, bool>> filter, params string[] includeProperties)
        {
            return await GetAllReadOnly(filter, null, null, null, includeProperties).FirstOrDefaultAsync();
        }

        public IQueryable<T> GetAll(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, int? skip = null, int? take = null, params string[] includeProperties)
        {
            IQueryable<T> queryable = Queryable.AsQueryable(dbSet);
            if (filter != null)
            {
                queryable = queryable.Where(filter);
            }

            if (includeProperties != null && includeProperties.Length != 0)
            {
                foreach (string navigationPropertyPath in includeProperties)
                {
                    queryable = queryable.Include(navigationPropertyPath);
                }
            }

            if (orderBy != null)
            {
                queryable = orderBy(queryable);
                if (skip.HasValue && skip.HasValue)
                {
                    queryable = queryable.Skip(skip.Value);
                }

                if (take.HasValue && take.HasValue)
                {
                    queryable = queryable.Take(take.Value);
                }
            }

            return queryable;
        }

        public IQueryable<T> GetAllReadOnly(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, int? skip = null, int? take = null, params string[] includeProperties)
        {
            return GetAll(filter, orderBy, skip, take, includeProperties).AsNoTracking();
        }

        private void save(params T[] entities)
        {
            foreach (T val in entities)
            {
                if (val.ID.ToString().Length > 0)
                {
                    update(val);
                }
                else
                {
                    dbSet.Add(val);
                }
            }
        }

        private void delete(params T[] entities)
        {
            dbSet.RemoveRange(entities);
        }

        private void update(T entity)
        {
            T val = dbSet.Local.FirstOrDefault((T d) => d.ID == entity.ID);
            if (val == null)
            {
                dbContext.Entry(entity).State = EntityState.Modified;
            }
            else
            {
                dbContext.Entry(val).CurrentValues.SetValues(entity);
            }
        }
    }
}
