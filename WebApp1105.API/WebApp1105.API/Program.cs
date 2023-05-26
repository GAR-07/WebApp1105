using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApp1105.API.Models;
using WebApp1105.Data.Interfaces;
using WebApp1105.Data.Repository;
using WebApp1105.Models;

var builder = WebApplication.CreateBuilder(args);

var authOptions = builder.Configuration.GetSection("AuthOptions").Get<AuthOptions>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddTransient<IAllAccounts, AccountRepository>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // ���������, ����� �� �������������� �������� ��� ��������� ������
            ValidIssuer = authOptions.Issuer, // ������, �������������� ��������
            ValidateAudience = true, // ����� �� �������������� ����������� ������
            ValidAudience = authOptions.Audience, // ��������� ����������� ������
            ValidateIssuerSigningKey = true, // ��������� ����� ������������
            IssuerSigningKey = authOptions.GetSymmetricSecurityKey(), // ��������� ����� ������������
            ValidateLifetime = true, // ����� �� �������������� ����� �������������
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
