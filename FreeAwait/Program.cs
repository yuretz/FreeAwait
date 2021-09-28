using System.Threading.Tasks;

namespace FreeAwait
{
	internal struct Program<TResult> : IInstruction<TResult>
	{
			public Program(FreeAwaiter<TResult> builder)
			{
				_builder = builder;
			}

			public FreeAwaiter<TResult> GetAwaiter() => _builder;

			public IInstruction<TResult> Use(IInterpreter interpreter) => _builder.Use(interpreter).Task;

			public async Task<TResult> Run(IInterpreter interpreter) => await Use(interpreter);


		private readonly FreeAwaiter<TResult> _builder;
	}
}
