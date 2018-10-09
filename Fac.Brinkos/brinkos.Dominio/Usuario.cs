using brinkos.Dominio.Core;

namespace brinkos.Dominio
{
    public class Usuario : Entity
    {
        public string UsuarioId { get; set; }
        public string Contrasena { get; set; }
    }
}