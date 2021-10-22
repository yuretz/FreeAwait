using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace FreeAwait
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRunner(this IServiceCollection services, Type type)
        {
            if(!type.IsAssignableTo(typeof(IRunner)))
            {
                throw new ArgumentException($"{type.Name} does not implement any known runner interface");
            }

            foreach (var runner in type.GetInterfaces()
                    .Where(item => item.IsGenericType
                        && item.GetGenericTypeDefinition() is var generic
                        && generic.Equals(typeof(IRunOne<,>))))
            {
                services.AddTransient(runner, type);
            }

            if(type.IsAssignableTo(typeof(IRunMany)))
            {
                services.AddTransient(type, typeof(IRunMany));
            }
        }

        public static void AddRunners(this IServiceCollection services, Assembly? assembly = default)
        {
            assembly ??= Assembly.GetCallingAssembly();

            foreach (var type in assembly.GetTypes()
                .Where(type => !(type.IsInterface || type.IsAbstract) && type.IsAssignableTo(typeof(IRunner))))
            {
                services.AddRunner(type);
            }
        }
    }
}
