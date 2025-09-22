using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployerApp.Api.Data;
using EmployerApp.Api.Models;

namespace EmployerApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeDocumentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EmployeeDocumentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetEmployeeDocuments(Guid employeeId)
    {
        var documents = await _context.EmployeeDocuments
            .Include(d => d.DocumentType)
            .Where(d => d.EmployeeId == employeeId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        var result = documents.Select(d => new
        {
            d.Id,
            d.DocumentName,
            d.FileSize,
            d.MimeType,
            d.CreatedAt,
            DocumentType = new
            {
                d.DocumentType.Id,
                d.DocumentType.TypeName
            }
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployeeDocument(Guid id)
    {
        var document = await _context.EmployeeDocuments
            .Include(d => d.DocumentType)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
        {
            return NotFound();
        }

        return File(document.FileData, document.MimeType, document.DocumentName);
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateEmployeeDocument([FromForm] CreateDocumentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var employee = await _context.Employees.FindAsync(request.EmployeeId);
        if (employee == null)
        {
            return NotFound("Сотрудник не найден");
        }

        var documentType = await _context.DocumentTypes.FindAsync(request.DocumentTypeId);
        if (documentType == null)
        {
            return NotFound("Тип документа не найден");
        }

        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest("Файл не выбран");
        }

        const int maxFileSize = 10 * 1024 * 1024; // 10MB
        if (request.File.Length > maxFileSize)
        {
            return BadRequest("Размер файла не должен превышать 10MB");
        }

        using var memoryStream = new MemoryStream();
        await request.File.CopyToAsync(memoryStream);

        var document = new EmployeeDocument
        {
            Id = Guid.NewGuid(),
            EmployeeId = request.EmployeeId,
            DocumentTypeId = request.DocumentTypeId,
            DocumentName = request.DocumentName ?? request.File.FileName,
            FileData = memoryStream.ToArray(),
            FileSize = (int)request.File.Length,
            MimeType = request.File.ContentType,
            CreatedAt = DateTime.UtcNow
        };

        _context.EmployeeDocuments.Add(document);
        await _context.SaveChangesAsync();

        var result = new
        {
            document.Id,
            document.DocumentName,
            document.FileSize,
            document.MimeType,
            document.CreatedAt,
            DocumentType = new
            {
                document.DocumentType.Id,
                document.DocumentType.TypeName
            }
        };

        return CreatedAtAction(nameof(GetEmployeeDocument), new { id = document.Id }, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployeeDocument(Guid id)
    {
        var document = await _context.EmployeeDocuments.FindAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        _context.EmployeeDocuments.Remove(document);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateDocumentRequest
{
    public Guid EmployeeId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string? DocumentName { get; set; }
    public IFormFile File { get; set; } = null!;
}

