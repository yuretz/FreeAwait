using System.Threading.Tasks;

namespace FreeAwait
{
    public interface IInterpreter
	{
	}

	public interface IInterpreter<in TInstruction, TResult> : IInterpreter
		where TInstruction : IInstruction<TResult>
	{
		Task<TResult> Run(TInstruction command);
	}
}
