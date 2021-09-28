using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FreeAwait
{
	[AsyncMethodBuilder(typeof(FreeAwaiter<>))]
	public interface IInstruction<TResult>
	{
		IInstruction<TResult> Use(IInterpreter interpreter) => GetAwaiter().Use(interpreter).Task;

		Task<TResult> Run(IInterpreter interpreter);

		FreeAwaiter<TResult> GetAwaiter() => new(this);
	}

	public interface IInstruction<TInstruction, TResult> : IInstruction<TResult>
		where TInstruction : IInstruction<TResult>
	{
		async Task<TResult> IInstruction<TResult>.Run(IInterpreter interpreter) => interpreter is IInterpreter<TInstruction, TResult> runner
			? await runner.Run((TInstruction)this)
			: throw new ArgumentException($"Interpreter ${interpreter.GetType().Name} doesn't accept {typeof(TInstruction).Name}");
	}

	public static class InstructionExtensions
	{
		public static IInstruction<TResult> Run<TResult>(this IInstruction<TResult> command) => command;
	}
}