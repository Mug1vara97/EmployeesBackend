using Microsoft.EntityFrameworkCore;
using EmployerApp.Api.Data;
using EmployerApp.Api.Models;
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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

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

app.UseAuthorization();

app.MapControllers();

app.Run();
