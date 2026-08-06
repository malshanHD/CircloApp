using CircloApp.Application.Interfaces;
using CircloApp.Infrastructure.Authentication;
using CircloApp.Infrastructure.Cache;
using CircloApp.Infrastructure.Email;
using CircloApp.Infrastructure.OTP;
using CircloApp.Infrastructure.Persistence;
using CircloApp.Infrastructure.Repositories;
using CircloApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

namespace CircloApp.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IDateTimeProvider, DateTimeProvider>();

            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
            services.AddScoped<ICacheService, RedisCacheService>();
            services.AddScoped<IOtpGenerator, OtpGenerator>();

            var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateIssuerSigningKey = true,
                            ValidateLifetime = true,
                            ValidIssuer = jwtSettings.Issuer,
                            ValidAudience = jwtSettings.Audience,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                        };
                    });

            services.Configure<RedisSettings>(configuration.GetSection(RedisSettings.SectionName));

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;
                return ConnectionMultiplexer.Connect(settings.ConnectionString);
            });

            services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IEventMemberRepository, EventMemberRepository>();

            return services;
        }
    }
}
