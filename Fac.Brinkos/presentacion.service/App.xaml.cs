using presentacion.service.Comunes;
using presentacion.service.Container;
using System.Windows;

namespace presentacion.service
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var commonService = (ICommonService)UnityInstanceProvider.GetInstance(typeof(ICommonService));

            new MainWindow().ShowDialog();
        }
    }
}