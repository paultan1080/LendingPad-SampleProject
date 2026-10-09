using System.Reflection;
using BusinessEntities;
using Common;
using SimpleInjector;

namespace Data
{
    public class DataConfiguration
    {
        public static void Initialize(Container container, Lifestyle lifestyle)
        {
            var assembly = typeof(DataConfiguration).Assembly;

            container.RegisterSingleton<IListTypeLookup<Assembly>, ListTypeLookup<Assembly>>();

            InitializeAssemblyInstancesService.RegisterAssemblyWithSerializableTypes(container, typeof(User).Assembly);
            InitializeAssemblyInstancesService.RegisterAssemblyWithSerializableTypes(container, assembly);

            // [AutoRegister]: the in-memory store (singleton), the request's session and UOW
            // (one scoped InMemorySession serves both) and the repositories.
            InitializeAssemblyInstancesService.Initialize(container, lifestyle, assembly);
        }
    }
}