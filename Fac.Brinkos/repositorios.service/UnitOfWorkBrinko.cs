using brinkos.Dominio;
using repositorios.service.Core;
using repositorios.service.Mapping;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace repositorios.service
{
    public class UnitOfWorkBrinko : BCUnitOfWork, IQueryableUnitOfWork
    {
        public UnitOfWorkBrinko()
            : base("connectionString")
        {
            Database.SetInitializer<UnitOfWorkBrinko>(null);
        }

        public IDbSet<Usuario> Usuario { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Configurations.Add(new UsuarioMap());
        }
    }
}