using brinkos.Dominio;
using brinkos.Dominio.Core;
using repositorios.service.Core;

namespace repositorios.service
{
    public class RepositorioGenerico<TEntity> : Repository<TEntity>, IRepositoryGenerico<TEntity>
         where TEntity : Entity
    {
        public RepositorioGenerico(UnitOfWorkBrinko unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}