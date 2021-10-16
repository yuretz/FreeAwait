using System;
using System.Threading.Tasks;

namespace FreeAwait
{
    internal struct Result<TResult> : IStep<TResult>
    {
        public Result(Planner<TResult> planner)
        {
            _planner = planner;
        }

        public Planner<TResult> GetAwaiter() => _planner;

        public IStep<TResult> Use(IRunner runner) => _planner.Use(runner).Task;

        public async IStep<TResult>? Run(IRunner runner, Action<TResult> next)
        {
            
            var result = _planner.IsCompleted ? _planner.GetResult() : await Use(runner);
            next(result);
            return result;
        }     

        private readonly Planner<TResult> _planner;
    }
}
