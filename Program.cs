using ArogyaPulse.Api.Data;
using ArogyaPulse.Api.Interfaces;
using ArogyaPulse.Api.Mapping;
using ArogyaPulse.Api.Middleware;
using ArogyaPulse.Api.Repositories;
using ArogyaPulse.Api.Services;
using ArogyaPulse.Api.Models;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection was not configured.")));
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.Configure<AiSettings>(builder.Configuration.GetSection("AI"));
builder.Services.AddHttpClient("Gemini", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<ITriageService, TriageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAiChatService, AiChatService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ISyncService, SyncService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
        else
        {
            var frontendUrl = builder.Configuration["Frontend:Url"];
            if (string.IsNullOrWhiteSpace(frontendUrl))
            {
                throw new InvalidOperationException("Frontend:Url must be configured in production.");
            }
            policy.WithOrigins(frontendUrl).AllowAnyMethod().AllowAnyHeader();
        }
    });
});
var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseCors("AllowFrontend");
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapGet("/api/health", async (AppDbContext db) =>
{
    var databaseHealthy = await db.Database.CanConnectAsync();
    return Results.Ok(new
    {
        status = databaseHealthy ? "healthy" : "degraded",
        database = databaseHealthy ? "healthy" : "unavailable",
        service = "ArogyaPulse.Api",
        version = "3.0.0",
        timestamp = DateTime.UtcNow
    });
});
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    SeedData.Initialize(db);
}
app.Run();