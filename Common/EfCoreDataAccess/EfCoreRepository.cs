using Common.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Common.EfCoreDataAccess
{
    public class EfCoreRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        internal EfCoreDbContext _context;
        internal DbSet<TEntity> DbSet;

        public EfCoreRepository(EfCoreDbContext context)
        {
            _context = context;
            DbSet = context.Set<TEntity>();
        }

        public async Task Insert(TEntity entity)
        {
            await DbSet.AddAsync(entity);
        }

        public async Task<bool> Update(TEntity entity)
        {
            try
            {
                DbSet.Attach(entity);
                _context.Entry(entity).State = EntityState.Modified;

                return await Task.FromResult(false);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> Delete(TEntity entity)
        {
            DbSet.Remove(entity);
            return await Task.FromResult(true);
        }

        public Task<TEntity> GetById(params object[] ids)
        {
            return DbSet.FindAsync(ids).AsTask();
        }

        public async Task<IReadOnlyCollection<TEntity>> GetFilteredList(Expression<Func<TEntity, bool>> filter)
        {
            return await DbSet.Where(filter).ToListAsync();
        }

        public async Task<TEntity> GetFirstOrDefaultWithIncludes(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] includePropertyExpressions)
        {
            return await AddMultipleIncludesToQuery(DbSet, includePropertyExpressions)
                .FirstOrDefaultAsync(filter);
        }

        protected IQueryable<TEntity> AddMultipleIncludesToQuery(IQueryable<TEntity> initialQuery, params Expression<Func<TEntity, object>>[] includePropertyExpressions)
        {
            IQueryable<TEntity> queryWithIncludes = includePropertyExpressions.Aggregate(initialQuery, (currentQuery, includeExpression) => currentQuery.Include(includeExpression));
            return queryWithIncludes;
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
