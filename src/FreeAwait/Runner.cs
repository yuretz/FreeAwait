using System;
using System.Threading.Tasks;

namespace FreeAwait
{
    public interface IRunner
    {
        void Run<TStep, TResult>(IStep<TStep, TResult> step, Action<TResult> next)
            where TStep : IStep<TStep, TResult> =>
            throw new NotSupportedException($"Runner {GetType().Name} doesn't accept {typeof(TStep).Name}");
    }

    public interface IRunAsync<in TStep, TResult> : IRunner
        where TStep : IStep<TStep, TResult>
    {
        Task<TResult> RunAsync(TStep step);
    }

    public interface IRun<in TStep, TResult> : IRunner
        where TStep : IStep<TStep, TResult>
    {
        TResult Run(TStep step);
    }

    public static class RunnerExt
    {
        public static async IStep<T> Run<T>(this IRunner runner, IStep<T> step) => await step.Use(runner);
    }

}
