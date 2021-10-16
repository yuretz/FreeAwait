using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
