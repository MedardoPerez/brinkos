using Microsoft.Practices.Unity;
using System;

namespace presentacion.service.Container
{
    public static class UnityInstanceProvider
    {
        #region Members

        private static readonly IUnityContainer _container;

        #endregion Members

        #region Constructor

        /// <summary>
        /// Create a new instance of unity instance provider
        /// </summary>
        static UnityInstanceProvider()
        {
            _container = Container.Current;
        }

        #endregion Constructor

        #region IInstance Provider Members

        public static object GetInstance(Type serviceType)
        {
            //This is the only call to UNITY container in the whole solution
            return _container.Resolve(serviceType);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="instance"></param>
        public static void ReleaseInstance(object instance)
        {
            var disposable = instance as IDisposable;
            if (disposable != null) (disposable).Dispose();
        }

        #endregion IInstance Provider Members
    }
}