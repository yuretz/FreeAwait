using System;
using System.Threading.Tasks;

namespace FreeAwait
{
    internal struct Plan<TResult> : IStep<TResult>
    {
        public Plan(Planner<TResult> planner)
        {
            _planner = planner;
        }

        public Planner<TResult> GetAwaiter() => _planner;

        public IStep<TResult> Use(IRunner runner) => _planner.Use(runner).Task;

        public async void Run(IRunner runner, Action<TResult> next) => next(await Use(runner));

        private readonly Planner<TResult> _planner;
    }
}
