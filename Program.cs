using Microsoft.EntityFrameworkCore;
using ArogyaPulse.Api.Data;
using ArogyaPulse.Api.Interfaces;
using ArogyaPulse.Api.Repositories;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();