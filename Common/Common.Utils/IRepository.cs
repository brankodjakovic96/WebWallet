using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utils
{
    public interface IRepository<TEntity> : IDisposable where TEntity : class
    {
        Task Insert(TEntity entity);
        Task<bool> Update(TEntity entity);
        Task<bool> Delete(TEntity entity);
        Task<TEntity> GetById(params object[] ids);
        Task<IReadOnlyCollection<TEntity>> GetFilteredList(Expression<Func<TEntity, bool>> filter);

    }
}
