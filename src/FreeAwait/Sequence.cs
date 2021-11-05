using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FreeAwait
{
    public class Sequence<T> : IStep<IAsyncEnumerable<T>>
    {
        public Sequence(IEnumerable<IStep<T>> steps)
        {
            _steps = steps;
        }

        public Planner<IAsyncEnumerable<T>> GetAwaiter() => new Planner<IAsyncEnumerable<T>>(this);

        public IStep<IAsyncEnumerable<T>>? Run(IRunner runner, Action<IAsyncEnumerable<T>> next)
        {
            next(new StepEnumerable<T>(_steps, runner));
            return null;
        }

        private readonly IEnumerable<IStep<T>> _steps;
    }

    public class StepEnumerable<T> : IAsyncEnumerable<T>
    {
        public StepEnumerable(IEnumerable<IStep<T>> steps, IRunner runner)
        {
            _steps = steps;
            _runner = runner;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            new StepEnumerator<T>(_steps.GetEnumerator(), _runner);


        private readonly IEnumerable<IStep<T>> _steps;
        private readonly IRunner _runner;
    }

    public class StepEnumerator<T> : IAsyncEnumerator<T>
    {
        public StepEnumerator(IEnumerator<IStep<T>> steps, IRunner runner)
        {
            _steps = steps;
            _runner = runner;
        }

        public T Current { get; private set; } = default!;

        public ValueTask DisposeAsync() => default;

        public async ValueTask<bool> MoveNextAsync()
        {
            var result = _steps.MoveNext();

            if (result)
            {
                Current = await _steps.Current.Use(_runner);
            }

            return result;
        }

        private readonly IEnumerator<IStep<T>> _steps;
        private readonly IRunner _runner;
    }
}
