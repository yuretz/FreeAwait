using System;

namespace FreeAwait
{
    internal struct Suspend<TResult>: IStep<TResult>
    {
        public Suspend(Func<IStep<TResult>> resume)
        {
            _resume = resume;
        }

        public IStep<TResult>? Run(IRunner runner, Action<TResult> next) =>
            _resume().Run(runner, next);
       
        public Planner<TResult> GetAwaiter() => new(this);

        private readonly Func<IStep<TResult>> _resume;
    }
}
