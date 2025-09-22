using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployerApp.Api.Data;
using EmployerApp.Api.Models;

namespace EmployerApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EmployeesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetEmployees()
    {
        var employees = await _context.Employees
            .Include(e => e.EmployeeDocuments)
            .ToListAsync();

        var result = employees.Select(e => new
        {
            e.Id,
            e.FirstName,
            e.LastName,
            e.MiddleName,
            e.Email,
            e.Phone,
            e.DateOfBirth,
            e.CreatedAt,
            e.UpdatedAt,
            FullName = e.GetFullName(),
            DocumentsCount = e.EmployeeDocuments.Count
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetEmployee(Guid id)
    {
        var employee = await _context.Employees
            .Include(e => e.EmployeeDocuments)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
        {
            return NotFound();
        }

        var result = new
        {
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employee.MiddleName,
            employee.Email,
            employee.Phone,
            employee.DateOfBirth,
            employee.CreatedAt,
            employee.UpdatedAt,
            FullName = employee.GetFullName(),
            DocumentsCount = employee.EmployeeDocuments.Count
        };

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateEmployee(Employee employee)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        employee.Id = Guid.NewGuid();
        employee.CreatedAt = DateTime.UtcNow;
        employee.UpdatedAt = DateTime.UtcNow;
        

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        var result = new
        {
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employee.MiddleName,
            employee.Email,
            employee.Phone,
            employee.DateOfBirth,
            employee.CreatedAt,
            employee.UpdatedAt,
            FullName = employee.GetFullName()
        };

        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(Guid id, Employee employee)
    {
        if (id != employee.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingEmployee = await _context.Employees.FindAsync(id);
        if (existingEmployee == null)
        {
            return NotFound();
        }

        existingEmployee.FirstName = employee.FirstName;
        existingEmployee.LastName = employee.LastName;
        existingEmployee.MiddleName = employee.MiddleName;
        existingEmployee.Email = employee.Email;
        existingEmployee.Phone = employee.Phone;
        existingEmployee.DateOfBirth = employee.DateOfBirth;
        existingEmployee.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EmployeeExists(id))
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
    public async Task<IActionResult> DeleteEmployee(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<object>>> SearchEmployees([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return await GetEmployees();
        }

        var employees = await _context.Employees
            .Include(e => e.EmployeeDocuments)
            .Where(e => 
                e.FirstName.Contains(q) || 
                e.LastName.Contains(q) || 
                e.MiddleName.Contains(q) ||
                (e.Email != null && e.Email.Contains(q)) ||
                (e.Phone != null && e.Phone.Contains(q)))
            .ToListAsync();

        var result = employees.Select(e => new
        {
            e.Id,
            e.FirstName,
            e.LastName,
            e.MiddleName,
            e.Email,
            e.Phone,
            e.DateOfBirth,
            e.CreatedAt,
            e.UpdatedAt,
            FullName = e.GetFullName(),
            DocumentsCount = e.EmployeeDocuments.Count
        });

        return Ok(result);
    }

    private bool EmployeeExists(Guid id)
    {
        return _context.Employees.Any(e => e.Id == id);
    }
}

