using Infrastructure.Services;
using Infrastructure.Setting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Tamkeen.Application.Interfaces;
using Tamkeen.Application.Interfaces.Auth;
using Tamkeen.Application.Interfaces.Feedback;
using Tamkeen.Application.Interfaces.Invitation;
using Tamkeen.Application.Interfaces.Manager;
using Tamkeen.Application.Interfaces.Payments;
using Tamkeen.Application.Interfaces.Ticket_Interface;
using Tamkeen.Application.Interfaces.Vendor;
using Tamkeen.Domain.Entities;
using Tamkeen.Infrastructure.Data;
using Tamkeen.Infrastructure.Implementation.Auth;
using Tamkeen.Infrastructure.Implementation.Invitation;
using Tamkeen.Infrastructure.Implementation.Manager;
using Tamkeen.Infrastructure.Implementation.Payments;
using Tamkeen.Infrastructure.Implementation.Ticket_Implementation;
using Tamkeen.Infrastructure.Services;
using Tamkeen.Infrastructure.Setting;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentityCore<AppUser>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireDigit = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSetting>()!;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = ctx =>
                {
                    var path = ctx.HttpContext.Request.Path;
                    var token = ctx.Request.Query["access_token"].ToString();
                    if (!string.IsNullOrEmpty(token) && path.StartsWithSegments("/hubs"))
                        ctx.Token = token;
                    return Task.CompletedTask;
                }
            };
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {

                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = "Tamkeen",
                ValidAudience = "TamkeenUsers",

                IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"])
        ),

                ClockSkew = TimeSpan.Zero,

               
                RoleClaimType = "role",
                NameClaimType = "sub"
            };
        });

        services.Configure<JwtSetting>(configuration.GetSection("Jwt"));
        services.Configure<EmailSetting>(configuration.GetSection("EmailSetting"));
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<ITicketApplicationService, TicketApplicationService>();
        services.AddScoped<IVendorService, VendorService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<IInvitationService, InvitationService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IManagerService,ManagerService>();
        services.Configure<PaymobSettings>(
        configuration.GetSection("Paymob"));
        services.AddHttpClient<PaymobService>();
        services.AddScoped<IPaymentService, PaymentService>();


        return services;
    }
}