namespace Template.WebApi.Middleware;

public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            AddHeaderIfMissing(context, "X-Content-Type-Options", "nosniff");
            AddHeaderIfMissing(context, "X-Frame-Options", "DENY");
            AddHeaderIfMissing(context, "Referrer-Policy", "no-referrer");

            return Task.CompletedTask;
        });

        await next(context);
    }

    private static void AddHeaderIfMissing(HttpContext context, string name, string value)
    {
        if (!context.Response.Headers.ContainsKey(name))
        {
            context.Response.Headers[name] = value;
        }
    }
}
