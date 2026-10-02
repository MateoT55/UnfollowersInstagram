using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using UnfollowersInstagram.Services;

var builder = WebApplication.CreateBuilder(args);

const long MaxRequestBytes = 10_000_000; // 10 MB por request (dos exports de Instagram)

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Services.AddScoped<UnfollowersService>();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = MaxRequestBytes;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = MaxRequestBytes;
});

// Solo el frontend de produccion (Vercel) y origenes locales en desarrollo pueden llamar a la API.
string[] origenesPermitidos = builder.Environment.IsDevelopment()
    ? new[] { "http://localhost:3000", "http://localhost:5173", "http://localhost:5500", "https://localhost:7159" }
    : new[] { "https://unfollowersinstagram.vercel.app" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.WithOrigins(origenesPermitidos)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Rate limit por IP: 10 requests/minuto contra el endpoint de scan.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("scan", context =>
    {
        string ip = ObtenerIpCliente(context);
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });

    options.OnRejected = async (context, _) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            "{\"error\":\"Demasiadas solicitudes. Esperá un minuto y volvé a intentar.\"}");
    };
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var logger = context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("ExceptionHandler");

        if (feature?.Error is Exception ex)
        {
            logger.LogError(ex, "Excepcion no controlada en {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\":\"Error interno del servidor\"}");
    });
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors("PermitirFrontend");

app.UseRateLimiter();

app.UseAuthorization();

// Cabeceras de seguridad en todas las respuestas
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "no-referrer";
    headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});

app.MapControllers();

app.Run();

static string ObtenerIpCliente(HttpContext context)
{
    // Atras de Cloudflare/Render el RemoteIpAddress es el proxy; el ultimo valor de
    // X-Forwarded-For lo agrega el proxy inmediato y es el cliente real.
    string forwarded = context.Request.Headers["X-Forwarded-For"].ToString();
    if (!string.IsNullOrWhiteSpace(forwarded))
    {
        string[] partes = forwarded.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (partes.Length > 0)
        {
            return partes[^1];
        }
    }

    return context.Connection.RemoteIpAddress?.ToString() ?? "desconocida";
}
