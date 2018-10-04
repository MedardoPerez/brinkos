using brinkos.Dominio;

namespace repositorios.service.Mapping
{
    internal class UsuarioMap : EntityConfiguration<Usuario>
    {
        public UsuarioMap()
        {
            ToTable("Usuario");
            HasKey(r => r.UsuarioId);
            Property(r => r.UsuarioId).HasColumnName("Usuario");
            Property(r => r.Contrasena).HasColumnName("Contrasena");
        }
    }
}