using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployerApp.Api.Data;
using EmployerApp.Api.Models;

namespace EmployerApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentTypesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DocumentTypesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetDocumentTypes()
    {
        var documentTypes = await _context.DocumentTypes
            .OrderBy(dt => dt.TypeName)
            .ToListAsync();

        var result = documentTypes.Select(dt => new
        {
            dt.Id,
            dt.TypeName,
            dt.CreatedAt,
            DocumentsCount = dt.EmployeeDocuments.Count
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetDocumentType(Guid id)
    {
        var documentType = await _context.DocumentTypes
            .Include(dt => dt.EmployeeDocuments)
            .FirstOrDefaultAsync(dt => dt.Id == id);

        if (documentType == null)
        {
            return NotFound();
        }

        var result = new
        {
            documentType.Id,
            documentType.TypeName,
            documentType.CreatedAt,
            DocumentsCount = documentType.EmployeeDocuments.Count
        };

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateDocumentType(DocumentType documentType)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        documentType.Id = Guid.NewGuid();
        documentType.CreatedAt = DateTime.UtcNow;

        _context.DocumentTypes.Add(documentType);
        await _context.SaveChangesAsync();

        var result = new
        {
            documentType.Id,
            documentType.TypeName,
            documentType.CreatedAt,
            DocumentsCount = 0
        };

        return CreatedAtAction(nameof(GetDocumentType), new { id = documentType.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDocumentType(Guid id, DocumentType documentType)
    {
        if (id != documentType.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingDocumentType = await _context.DocumentTypes.FindAsync(id);
        if (existingDocumentType == null)
        {
            return NotFound();
        }

        existingDocumentType.TypeName = documentType.TypeName;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DocumentTypeExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocumentType(Guid id)
    {
        var documentType = await _context.DocumentTypes.FindAsync(id);
        if (documentType == null)
        {
            return NotFound();
        }

        var hasDocuments = await _context.EmployeeDocuments
            .AnyAsync(ed => ed.DocumentTypeId == id);

        if (hasDocuments)
        {
            return BadRequest("Нельзя удалить тип документа, который используется в документах сотрудников");
        }

        _context.DocumentTypes.Remove(documentType);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool DocumentTypeExists(Guid id)
    {
        return _context.DocumentTypes.Any(dt => dt.Id == id);
    }
}

