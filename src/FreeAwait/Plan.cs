using System.Threading.Tasks;

namespace FreeAwait
{
	internal struct Plan<TResult> : IStep<TResult>
	{
		public Plan(Planner<TResult> builder)
		{
			_builder = builder;
		}

		public Planner<TResult> GetAwaiter() => _builder;

		public IStep<TResult> Use(IRun runner) => _builder.Use(runner).Task;

		public async Task<TResult> Run(IRun runner) => await Use(runner);

		private readonly Planner<TResult> _builder;
	}
}
