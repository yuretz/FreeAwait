using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FreeAwait
{
    public readonly struct Void { }

    public interface IStep 
    {
        IStep<object?> Use(IRunner runner);
    }

    [AsyncMethodBuilder(typeof(Planner<>))]
    public interface IStep<TResult>: IStep
    {
        async IStep<object?> IStep.Use(IRunner runner) => await Use(runner);
        
        new IStep<TResult> Use(IRunner runner) => GetAwaiter().Use(runner).Task;

        IStep<TResult>? Run(IRunner runner, Action<TResult> next);

        Planner<TResult> GetAwaiter();
    }

    public interface IStep<TStep, TResult> : IStep<TResult>
        where TStep : IStep<TStep, TResult>
    {
        IStep<TResult>? IStep<TResult>.Run(IRunner runner, Action<TResult> next) =>
            runner.Run((TStep)this, next);

        Planner<TResult> IStep<TResult>.GetAwaiter() => new(this);
    }

    public static class Step
    {
        public static Planner<TResult> GetAwaiter<TStep, TResult>(this IStep<TStep, TResult> step)
            where TStep : IStep<TStep, TResult> => step.GetAwaiter();

        

        public static IStep<TResult> Result<TResult>(TResult value)
        {
            var planner = new Planner<TResult>();
            planner.SetResult(value);
            return planner.Task;
        }

        public static async IStep<TNext> PassTo<TResult, TNext>(
            this IStep<TResult> step, 
            Func<TResult, IStep<TNext>> next) => await next(await step);

        public static IStep<TResult> Suspend<TResult>(Func<IStep<TResult>> resume) => 
            new Suspend<TResult>(resume);

        public static IStep<IAsyncEnumerable<T>> Sequence<T>(this IEnumerable<IStep<T>> items) =>
            new Sequence<T>(items);
    }
}