#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace FreeAwait 
{
    public class RunResult : IResult
    {
        public RunResult(IStep step)
        {
            _step = step;
        }

        public async Task ExecuteAsync(HttpContext httpContext)
        {
            var runner = httpContext.RequestServices.GetRequiredService<IServiceRunner>();
            var value = await _step.Use(runner);
            var result = (value as IResult) ?? Results.Ok(value);
            await result.ExecuteAsync(httpContext);
        }

        private readonly IStep _step;
    }

    public static class ResultExtensions
    {
        public static IResult Run(this IResultExtensions _, IStep step) => new RunResult(step);
    }
}
#endif