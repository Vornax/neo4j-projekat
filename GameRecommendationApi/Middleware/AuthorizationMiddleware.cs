namespace GameRecommendationApi.Middleware
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _apiKey;

        public AuthorizationMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _apiKey = configuration["Authorization:ApiKey"] ?? "secret123";
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Preskoči Swagger
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            // Čita API ključ iz Authorization header-a
            var apiKey = context.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(apiKey) || apiKey != _apiKey)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            await _next(context);
        }
    }
}
