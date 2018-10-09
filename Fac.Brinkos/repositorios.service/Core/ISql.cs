using System.Collections.Generic;

namespace repositorios.service.Core
{
    public interface ISql
    {
        IEnumerable<TEntity> ExecuteQuery<TEntity>(string sqlQuery, params object[] parameters);
    }
}