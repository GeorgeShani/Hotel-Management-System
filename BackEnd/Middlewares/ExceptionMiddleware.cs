using System.Net;

namespace BackEnd.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                // Let the request flow through the pipeline (towards the controller)
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                // If any error occurred downstream, catch it here
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // By default we assume it's the server's fault (500 Internal Server Error)
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            string message = "An internal server error occurred.";

            // If the error comes from our business logic (e.g. "the hotel cannot be deleted because it has rooms")
            if (exception is InvalidOperationException || exception is ArgumentException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest; // 400 Bad Request
                message = exception.Message;
            }
            // If something was not found
            else if (exception.Message.Contains("not found"))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound; // 404 Not Found
                message = exception.Message;
            }

            // Format our standard response
            var response = new ErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                Message = message
            };

            return context.Response.WriteAsync(response.ToString());
        }
    }
}
