using System.ComponentModel.DataAnnotations;

namespace EmployerApp.Api.Models;

public class Employee
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? MiddleName { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<EmployeeDocument> EmployeeDocuments { get; set; } = new List<EmployeeDocument>();

    public string GetFullName()
    {
        return string.IsNullOrEmpty(MiddleName) 
            ? $"{FirstName} {LastName}"
            : $"{FirstName} {MiddleName} {LastName}";
    }
}








