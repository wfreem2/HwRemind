using Microsoft.EntityFrameworkCore;

namespace HwRemind.Middleware
{
    /// <summary>
    /// Will handle exceptions thrown anywhere within the application
    /// </summary>
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, IServiceProvider serviceProvider, 
            ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch(DbUpdateException e)
            {
                _logger.LogError("An error occured updating the database " + e.Message);
                context.Response.StatusCode = 500;
            }
            catch(Exception e)
            {
                _logger.LogError("An exception occured " + e.Message);
                context.Response.StatusCode = 500;
            }
        }
        
    }
}
