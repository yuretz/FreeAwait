using System;
using System.Threading.Tasks;

namespace FreeAwait
{
    public interface IRunner
    {
        IStep<TResult>? Run<TStep, TResult>(TStep step, Action<TResult> next)
            where TStep : IStep<TStep, TResult> =>
            this switch
            {
                IRunOne<TStep, TResult> runOne =>
                    runOne.RunOne(step, next),

                IRunMany runMany when runMany.Supports(step) =>
                    runMany.Run(step, result => next((TResult)result)) as IStep<TResult>,

                _ => throw new NotSupportedException($"Runner {GetType().Name} doesn't accept {typeof(TStep).Name}")
            };
    }

    public interface IRunMany: IRunner
    {
        bool Supports(IStep step) => true;
        IStep? Run(IStep step, Action<object> next);
    }

    public interface IRunOne<in TStep, TResult>: IRunner
        where TStep : IStep<TStep, TResult>
    {
        IStep<TResult>? RunOne(TStep step, Action<TResult> next);
    }

    public interface IRunAsync<in TStep, TResult> : IRunOne<TStep, TResult>
        where TStep : IStep<TStep, TResult>
    {
        IStep<TResult>? IRunOne<TStep, TResult>.RunOne(TStep step, Action<TResult> next)
        {
            RunAsync(step).ContinueWith(task => next(task.Result));
            return default;
        }

        Task<TResult> RunAsync(TStep step);
    }

    public interface IRun<in TStep, TResult> : IRunOne<TStep, TResult>
        where TStep : IStep<TStep, TResult>
    {
        IStep<TResult>? IRunOne<TStep, TResult>.RunOne(TStep step, Action<TResult> next)
        {
            next(Run(step));
            return default;
        }    

        TResult Run(TStep step);
    }

    public interface IRunStep<in TStep, TResult> : IRunOne<TStep, TResult>
        where TStep : IStep<TStep, TResult>
    {
        IStep<TResult>? IRunOne<TStep, TResult>.RunOne(TStep step, Action<TResult> next) => RunStep(step);

        IStep <TResult> RunStep(TStep step);
    }

    public static class RunnerExt
    {
        public static IStep<T> Run<T>(this IRunner runner, IStep<T> step) => step.Use(runner);
    }

}
