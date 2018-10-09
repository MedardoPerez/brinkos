using brinkos.Dominio.Core;
using System.Data.Entity;

namespace repositorios.service.Core
{
    public interface IQueryableUnitOfWork : IUnitOfWork, ISql
    {
        /// <summary>
        /// Returns a IDbSet instance for access to entities of the given type in the context,
        /// the ObjectStateManager, and the underlying store.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        DbSet<TEntity> CreateSet<TEntity>() where TEntity : class;

        void Attach<TEntity>(TEntity item) where TEntity : class;
    }
}