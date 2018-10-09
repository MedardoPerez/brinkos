using brinkos.Dominio.Core;

namespace brinkos.Dominio
{
    public interface IRepositoryGenerico<TEntity> : IRepository<TEntity>
        where TEntity : Entity
    {
    }
}