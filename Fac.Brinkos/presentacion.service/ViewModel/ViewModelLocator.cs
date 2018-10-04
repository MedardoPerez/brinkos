using brinkos.AppService;
using presentacion.service.Comunes;
using presentacion.service.Container;

namespace presentacion.service.ViewModel
{
    public class ViewModelLocator
    {
        private ICommonService CommonService
        {
            get
            {
                return _commonService ??
                       (_commonService = (ICommonService)UnityInstanceProvider.GetInstance(typeof(ICommonService)));
            }
        }

        private ICommonService _commonService;

        private ILoginServicioAplicacion LoginServicioAplicacion
        {
            get
            {
                return _loginServicioAplicacion ??
                    (_loginServicioAplicacion = (ILoginServicioAplicacion)UnityInstanceProvider.GetInstance(typeof(ILoginServicioAplicacion)));
            }
        }

        private ILoginServicioAplicacion _loginServicioAplicacion;

        public MainWindowViewModel MainWindowViewModel
        {
            get
            {
                return _mainWindowViewModel ??
                    (_mainWindowViewModel = new MainWindowViewModel(LoginServicioAplicacion));
            }
        }

        private MainWindowViewModel _mainWindowViewModel;
    }
}