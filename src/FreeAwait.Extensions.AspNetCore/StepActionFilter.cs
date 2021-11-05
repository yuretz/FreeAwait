using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace FreeAwait
{
    public class StepActionFilter : IAsyncActionFilter, IOrderedFilter
    {
        public StepActionFilter(IServiceRunner runner)
        {
            _runner = runner ?? throw new ArgumentNullException(nameof(runner));
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if(context.ActionDescriptor is ControllerActionDescriptor descriptor
                && descriptor.MethodInfo.ReturnType.IsAssignableTo(typeof(IStep)))
            {
                context.Result = await Execute(
                    context.Controller,
                    descriptor.MethodInfo,
                    descriptor.Parameters.Select(item => context.ActionArguments[item.Name]).ToArray());
            }
            else
            {
                _ = await next();
            }
        }

        public int Order => int.MaxValue;

        private async IStep<IActionResult> Execute(object controller, MethodInfo method, object?[]? parameters)
        {
            var step = (IStep)(method.Invoke(controller, parameters) ?? throw new InvalidOperationException("Step cannot be null"));
            return await step.Use(_runner) as IActionResult 
                ?? throw new InvalidOperationException("Step result is not IActionResult");
        }

        private readonly IServiceRunner _runner;        
    }

}
