using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FreeAwait
{
    [AsyncMethodBuilder(typeof(Planner<>))]
    public interface IStep<TResult>
    {
        IStep<TResult> Use(IRunner runner) => GetAwaiter().Use(runner).Task;

        IStep<TResult> Run(IRunner runner, Action<TResult> next);

        Planner<TResult> GetAwaiter();
    }

    public interface IStep<TStep, TResult> : IStep<TResult>
        where TStep : IStep<TStep, TResult>
    {
        IStep<TResult> IStep<TResult>.Run(IRunner runner, Action<TResult> next)
        {
            switch (runner)
            {
                case IRunAsync<TStep, TResult> runAsync:
                    runAsync.RunAsync((TStep)this).ContinueWith(task => next(task.Result));
                    break;

                case IRun<TStep, TResult> run:
                    next(run.Run((TStep)this));
                    break;

                default: throw new NotSupportedException($"Runner ${runner.GetType().Name} doesn't accept {typeof(TStep).Name}");
            }
            return this;
        }

        Planner<TResult> IStep<TResult>.GetAwaiter() => new(this);
    }


    public static class StepExtensions
    {
        public static Planner<TResult> GetAwaiter<TStep, TResult>(this IStep<TStep, TResult> step)
            where TStep : IStep<TStep, TResult> => step.GetAwaiter();
    }
}