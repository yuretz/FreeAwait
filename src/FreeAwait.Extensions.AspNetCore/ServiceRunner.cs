using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace FreeAwait
{
    public interface IServiceRunner: IRunner
    {
    }

    public class ServiceRunner: IServiceRunner
    {
        public ServiceRunner(IServiceProvider provider) 
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public IStep<TResult>? Run<TStep, TResult>(TStep step, Action<TResult> next)
            where TStep : IStep<TStep, TResult> =>
            ((IRunner?)_provider.GetService<IRunOne<TStep, TResult>>()
                ?? _provider.GetServices<IRunMany>().FirstOrDefault(item => item.Supports(step))
                ?? throw new NotSupportedException($"No runner found for {typeof(TStep).Name}"))
                .Run(step, next);

        private readonly IServiceProvider _provider;
    }
}
