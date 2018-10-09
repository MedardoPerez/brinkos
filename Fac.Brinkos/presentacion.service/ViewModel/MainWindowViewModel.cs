using brinkos.AppService;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace presentacion.service.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ILoginServicioAplicacion _loginServicioAplicacion;

        public MainWindowViewModel(ILoginServicioAplicacion loginServicioAplicacion)
        {
            _loginServicioAplicacion = loginServicioAplicacion;

            _loginServicioAplicacion.ObtenerUsuarios();
        }
    }
}