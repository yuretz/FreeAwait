using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FreeAwait
{
    [AsyncMethodBuilder(typeof(Planner<>))]
    public interface IStep<TResult>
    {
        IStep<TResult> Use(IRun runner) => GetAwaiter().Use(runner).Task;

        Task<TResult> Run(IRun runner);

        Planner<TResult> GetAwaiter() => new(this);
    }

    public interface IStep<TStep, TResult> : IStep<TResult>
        where TStep : IStep<TResult>
    {
        async Task<TResult> IStep<TResult>.Run(IRun runner) => runner is IRun<TStep, TResult> supported
            ? await supported.Run((TStep)this)
            : throw new NotSupportedException($"Runner ${runner.GetType().Name} doesn't accept {typeof(TStep).Name}");

        public IStep<TResult> Task => this;
    }

    public static class StepExtensions
    {
        public static IStep<TResult> Run<TResult>(this IStep<TResult> step) => step;
    }
}