using System.Threading.Tasks;

namespace FreeAwait
{
    public interface IRun
    {
    }

    public interface IRun<in TStep, TResult> : IRun
        where TStep : IStep<TResult>
    {
        Task<TResult> Run(TStep step);
    }
}
