using brinkos.AppService.DTO.Login;
using System.Collections.Generic;

namespace brinkos.AppService
{
    public interface ILoginServicioAplicacion
    {
        List<UsuarioDTO> ObtenerUsuarios();
    }
}