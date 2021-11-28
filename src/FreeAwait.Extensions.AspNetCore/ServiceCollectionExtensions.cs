using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FreeAwait
{
    public class RunnerOptions
    {
        public IList<Assembly> Assemblies { get; } = new List<Assembly>();
        public bool RegisterServiceRunner { get; set; }
        public bool AddGlobalStepFilter { get; set; }
    }
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRunner(this IServiceCollection services, Type type)
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

            return services;
        }

        public static IServiceCollection AddRunner<T>(this IServiceCollection services) where T : IRunner =>
            AddRunner(services, typeof(T));

        public static IServiceCollection AddFreeAwait(
            this IServiceCollection services, 
            Action<RunnerOptions>? configure = default)
        {
            var options = new RunnerOptions 
            { 
                RegisterServiceRunner = true,
                AddGlobalStepFilter = true
            };

            options.Assemblies.Add(Assembly.GetCallingAssembly());
            configure?.Invoke(options);

            foreach (var assembly in options.Assemblies)
            {
                foreach (var type in assembly.GetTypes()
                    .Where(type => !(type.IsInterface || type.IsAbstract) && type.IsAssignableTo(typeof(IRunner))))
                {
                    services.AddRunner(type);
                }
            }

#if NET6_0_OR_GREATER
            services.AddRunner<HttpSteps.Runner>();
#endif

            if (options.RegisterServiceRunner)
            {
                services.AddSingleton<IServiceRunner, ServiceRunner>();
            }



            services.AddSingleton<StepActionFilter>();
            if(options.AddGlobalStepFilter)
            {
                services.Configure<MvcOptions>(options => options.Filters.Add<StepActionFilter>());
            }

            return services;
        }

    }
}
