using Microsoft.EntityFrameworkCore;
using ArogyaPulse.Api.Data;
using ArogyaPulse.Api.Interfaces;
using ArogyaPulse.Api.Repositories;
using ArogyaPulse.Api.Services;
using ArogyaPulse.Api.Mapping;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "Connection string 'DefaultConnection' not found."
        )
    )
);
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<ITriageService, TriageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAiChatService, AiChatService>();
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
            policy.WithOrigins("https://your-frontend-domain.com").AllowAnyMethod().AllowAnyHeader();
        }
    });
});
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    SeedData.Initialize(db);
}
app.Run();