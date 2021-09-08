namespace AwsHelloWorldWeb
{
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Logging;

    public class ExceptionFilter : IAsyncExceptionFilter
    {
        private readonly ILogger<ExceptionFilter> _logger;

        public ExceptionFilter(ILogger<ExceptionFilter> logger)
        {
            _logger = logger;
        }
        
        public Task OnExceptionAsync(ExceptionContext context)
        {
            context.ExceptionHandled = true;
            var problem = new ActionResult<ProblemDetails>(new ProblemDetails()
            {
                Detail = context.Exception.ToString(),
                Status = (int)HttpStatusCode.InternalServerError,
                Title = context.Exception.Message,
                Type = context.Exception.GetType().ToString()
            });
            context.Result = problem.Result;

            _logger.LogError(context.Exception, "Error Encountered");
            
            return Task.CompletedTask;
        }
    }
}