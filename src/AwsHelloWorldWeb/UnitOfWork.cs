namespace AwsHelloWorldWeb
{
    using System;
    using System.Threading.Tasks;
    using Features.Values;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The UnitOfWork for keeping track of all database transactions.
    /// </summary>
    public class UnitOfWork : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<ValuesContext>();

            try
            {
                await dbContext.BeginTransactionAsync();

                var actionExecuted = await next();
                if (actionExecuted.Exception != null && !actionExecuted.ExceptionHandled)
                {
                    dbContext.RollbackTransaction();
                }
                else
                {
                    await dbContext.CommitTransactionAsync();
                }
            }
            catch
            {
                dbContext.RollbackTransaction();
                throw;
            }
        }
    }}