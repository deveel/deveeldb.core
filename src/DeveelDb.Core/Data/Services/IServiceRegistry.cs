using System;

namespace Deveel.Data.Services {
    public interface IServiceRegistry {
        void Register(ServiceRegistration registration);

        bool Unregister(Type serviceType, object serviceKey);

        bool IsRegistered(Type serviceType, object serviceKey);

        IServiceProvider BuildProvider();
    }
}