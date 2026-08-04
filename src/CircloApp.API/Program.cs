using CircloApp.API.Extensions;
using CircloApp.Application;
using CircloApp.Infrastructure;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CircloApp API",
        Version = "v1"
    });

    // 1. Define the Bearer Authentication Scheme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT bearer token below."
    });

    // 2. Apply Security Requirement (New .NET 10 / OpenAPI v2 syntax)
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger if running locally OR if explicitly enabled via configuration (Azure)
if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("EnableSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseGlobalExceptionMiddleware();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health/Status root route for easy verification
app.MapGet("/", () => Results.Ok(new { Status = "Healthy", App = "CircloApp API" }));

app.Run();