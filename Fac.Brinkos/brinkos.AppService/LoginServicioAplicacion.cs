using brinkos.AppService.DTO.Login;
using brinkos.Dominio;
using System.Collections.Generic;
using System.Linq;

namespace brinkos.AppService
{
    internal class LoginServicioAplicacion : ILoginServicioAplicacion
    {
        private readonly IRepositoryGenerico<Usuario> _usuarioRepositorio;

        public LoginServicioAplicacion(IRepositoryGenerico<Usuario> usuarioRepositorio)
        {
            _usuarioRepositorio = usuarioRepositorio;
        }

        public List<UsuarioDTO> ObtenerUsuarios()
        {
            var usuarios = _usuarioRepositorio.GetAll();

            return usuarios.Select(r =>
                new UsuarioDTO
                {
                    UsuarioId = r.UsuarioId,
                    Contrasena = r.Contrasena
                }).ToList();
        }
    }
}