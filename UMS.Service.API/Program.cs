using UMS.Core.AutoMapper;
using UMS.Core.Dto;
using UMS.Service.API.Middleware;
using AutoMapper;
using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);


var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));



builder.Services.AddAutoMapper(config => config.AddProfile<EntityDtoMapper>()); //register auto mapper
builder.Services.AddInfrastructure(builder.Configuration);// add db connection
builder.Services.AddServices();
builder.Services.AddRepositories();
builder.Services.ConfigureJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
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
});


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();
app.UseCustomLocalization();

app.MapControllers();

app.Run();