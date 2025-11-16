using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Serilog;
using UMS.Core.AutoMapper;
using UMS.Core.Dto;
using UMS.Service.API.Middleware;

// Configure Serilog early to catch startup errors
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting UMS Service API");

    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog from appsettings.json
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());
        


var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));



builder.Services.AddAutoMapper(config => config.AddProfile<EntityDtoMapper>()); //register auto mapper
builder.Services.AddInfrastructure(builder.Configuration);// add db connection
builder.Services.AddServices();
builder.Services.AddRepositories();
builder.Services.ConfigureJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// Register custom authorization middleware result handler for custom 403/401 messages
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllers()
    .AddDataAnnotationsLocalization()
    .AddViewLocalization();

var supportedCultures = new[] { "en", "ar" };

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SetDefaultCulture("en");
    options.AddSupportedCultures(supportedCultures);
    options.AddSupportedUICultures(supportedCultures);
    
    // Configure culture providers - query string has highest priority
    options.RequestCultureProviders.Clear();
   
    // No other changes are needed as the HeaderRequestCultureProvider is part of the Microsoft.AspNetCore.Localization.Routing namespace.
    options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
    // No other changes are needed as the HeaderRequestCultureProvider is part of the Microsoft.AspNetCore.Localization.Routing namespace.
    
});

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });
    builder.Services.AddCors(options => options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

    var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Global exception handling - should be early in pipeline to catch all exceptions
app.UseGlobalExceptionHandling();

// Use RequestLocalization middleware - reads from query string, header, etc.
var localizationOptions = app.Services.GetService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(localizationOptions!.Value);

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();


    // Apply the CORS policy
    app.UseCors("AllowAll");
    app.MapControllers();

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}