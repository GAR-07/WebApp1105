using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApp1105.API.Models;

var builder = WebApplication.CreateBuilder(args);

var authOptions = builder.Configuration.GetSection("AuthOptions").Get<AuthOptions>();

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // указывает, будет ли валидироваться издатель при валидации токена
            ValidIssuer = authOptions.Issuer, // строка, представляющая издателя
            ValidateAudience = true, // будет ли валидироваться потребитель токена
            ValidAudience = authOptions.Audience, // установка потребителя токена
            ValidateIssuerSigningKey = true, // валидация ключа безопасности
            IssuerSigningKey = authOptions.GetSymmetricSecurityKey(), // установка ключа безопасности
            ValidateLifetime = true, // будет ли валидироваться время существования
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder => builder
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithOrigins("http://localhost:4200")
            .AllowCredentials()
    );
});
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
