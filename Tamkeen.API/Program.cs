using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Tamkeen.Application.DependencyInjection;
using Tamkeen.Domain.Entities;
using Tamkeen.Infrastructure.Hubs;
using Tamkeen.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("https://sunna3.vercel.app")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();

builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await IdentitySeeder.SeedAsync(userManager, roleManager);
}
app.Run();