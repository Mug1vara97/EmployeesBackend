using System.Text;
using Microsoft.EntityFrameworkCore;
using EmployerApp.Api.Data;
using EmployerApp.Api.Models;
using EmployerApp.Api.Options;
using EmployerApp.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EmployerApp API",
        Version = "v1",
        Description = "API для управления сотрудниками"
    });
});

builder.Services.AddOpenApi();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(opts =>
{
    opts.SignIn.RequireConfirmedAccount = false;
    opts.SignIn.RequireConfirmedEmail = false;
}).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetRequiredSection(nameof(JwtSettings)).Get<JwtSettings>()!;
        
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
        };
        options.TokenValidationParameters = tokenValidationParameters;
    });

builder.Services.AddTransient<ITokenGenerator, TokenGenerator>();
builder.Services.AddTransient<ILoginHashService, LoginHashService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    if (!context.DocumentTypes.Any())
    {
        var defaultDocumentTypes = new[]
        {
            new DocumentType { Id = Guid.NewGuid(), TypeName = "Паспорт", CreatedAt = DateTime.UtcNow },
            new DocumentType { Id = Guid.NewGuid(), TypeName = "Трудовая книжка", CreatedAt = DateTime.UtcNow },
            new DocumentType { Id = Guid.NewGuid(), TypeName = "Диплом", CreatedAt = DateTime.UtcNow },
            new DocumentType { Id = Guid.NewGuid(), TypeName = "Справка о доходах", CreatedAt = DateTime.UtcNow },
            new DocumentType { Id = Guid.NewGuid(), TypeName = "Медицинская справка", CreatedAt = DateTime.UtcNow },
            new DocumentType { Id = Guid.NewGuid(), TypeName = "Фото", CreatedAt = DateTime.UtcNow },
            new DocumentType { Id = Guid.NewGuid(), TypeName = "Другое", CreatedAt = DateTime.UtcNow }
        };
        
        context.DocumentTypes.AddRange(defaultDocumentTypes);
        context.SaveChanges();
    }
}


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
