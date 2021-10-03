using System.Threading.Tasks;

namespace FreeAwait
{
    public interface IRunner
    {
    }

    public interface IRunAsync<in TStep, TResult> : IRunner
        where TStep : IStep<TResult>
    {
        Task<TResult> RunAsync(TStep step);
    }

    public interface IRun<in TStep, TResult> : IRunner
        where TStep : IStep<TResult>
    {
        TResult Run(TStep step);
    }
}
