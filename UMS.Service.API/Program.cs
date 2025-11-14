using UMS.Service.API.Middleware;
using UMS.Service.API.Models;

var builder = WebApplication.CreateBuilder(args);


var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

//// Register services
//builder.Services.AddScoped<IJwtService, JwtService>();
//builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddServices();
builder.Services.ConfigureJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();

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

app.MapControllers();

app.Run();