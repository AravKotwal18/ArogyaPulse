using Microsoft.EntityFrameworkCore;
using ArogyaPulse.Api.Data;
using ArogyaPulse.Api.Interfaces;
using ArogyaPulse.Api.Repositories;
using ArogyaPulse.Api.Interfaces;
using ArogyaPulse.Api.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<ITriageService, TriageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();