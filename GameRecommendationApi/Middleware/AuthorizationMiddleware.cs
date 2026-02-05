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
            // Preskoči Swagger i javne statičke resurse (images, css, js, favicon)
            var path = context.Request.Path;
            if (path.StartsWithSegments("/swagger") ||
                path.StartsWithSegments("/images") ||
                path.StartsWithSegments("/favicon.ico") ||
                path.StartsWithSegments("/css") ||
                path.StartsWithSegments("/js"))
            {
                await _next(context);
                return;
            }

            // Ako je GET i izgleda kao zahtev za statički fajl (ima ekstenziju), preskoči autorizaciju
            if (context.Request.Method == HttpMethods.Get &&
                System.IO.Path.HasExtension(path.Value ?? string.Empty))
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
