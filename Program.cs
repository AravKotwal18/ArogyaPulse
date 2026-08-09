using Microsoft.EntityFrameworkCore;
using ArogyaPulse.Api.Data;
using ArogyaPulse.Api.Repositories;
using ArogyaPulse.Api.Services;
using ArogyaPulse.Api.Mapping;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<ITriageService, TriageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
var app = builder.Build();
app.UseCors("AllowAll");
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    SeedData.Initialize(db);
}
app.Run();
