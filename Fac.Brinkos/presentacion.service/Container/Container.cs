using brinkos.AppService;
using brinkos.Dominio;
using Microsoft.Practices.Unity;
using presentacion.service.Comunes;
using repositorios.service;

namespace presentacion.service.Container
{
    public static class Container
    {
        private static IUnityContainer _currentContainer;

        public static IUnityContainer Current
        {
            get
            {
                return _currentContainer;
            }
        }

        static Container()
        {
            ConfigureContainer();

            ConfigureFactories();
        }

        private static void ConfigureFactories()
        {
        }

        private static void ConfigureContainer()
        {
            _currentContainer = new UnityContainer();

            _currentContainer.RegisterType<ICommonService, CommonService>();

            RegisterModuleDependencies();
        }

        private static void RegisterModuleDependencies()
        {
            //-> Unit of Work
            RegisterUnitofWorks(_currentContainer);

            //-> Repositories
            RegisterRepositories(_currentContainer);

            //-> Application services
            RegisterApplicationServices(_currentContainer);

            //-> Domain services
            RegisterDomainServices(_currentContainer);
        }

        private static void RegisterDomainServices(IUnityContainer _currentContainer)
        {
        }

        private static void RegisterUnitofWorks(IUnityContainer container)
        {
            container.RegisterType(typeof(UnitOfWorkBrinko), new ContainerControlledLifetimeManager());
        }

        private static void RegisterRepositories(IUnityContainer container)
        {
            container.RegisterType(typeof(IRepositoryGenerico<>), typeof(RepositorioGenerico<>));
        }

        private static void RegisterApplicationServices(IUnityContainer container)
        {
            container.RegisterType<ILoginServicioAplicacion, ILoginServicioAplicacion>();
        }
    }
}