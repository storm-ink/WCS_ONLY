using Wcs.WebApi.Hubs;
using Wcs.WebApi.OpenApi;

const string CorsPolicyName = "WcsWebCors";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (origins is { Length: > 0 } && !origins.Contains("*"))
        {
            policy.WithOrigins(origins)
                .AllowCredentials();
        }
        else
        {
            policy.AllowAnyOrigin();
        }

        policy.AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors(CorsPolicyName);

app.MapGet("/", () => Results.Redirect("/health"));
app.MapGet("/health", () => Results.Ok(new
{
    service = "Wcs.WebApi",
    status = "Healthy",
    timestamp = DateTimeOffset.UtcNow
}));

app.MapGet("/openapi/v1.json", (IConfiguration configuration) =>
    Results.Json(OpenApiDocumentFactory.Create(configuration)));
app.MapGet("/swagger", () =>
    Results.Content(OpenApiDocumentFactory.CreateSwaggerUiHtml("/openapi/v1.json"), "text/html"));

app.MapControllers();
app.MapHub<DeviceHub>("/hubs/device");
app.MapHub<TaskHub>("/hubs/task");
app.MapHub<AlarmHub>("/hubs/alarm");

app.Run();
