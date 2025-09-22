using System.ComponentModel.DataAnnotations;

namespace EmployerApp.Api.Models;

public class DocumentType
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string TypeName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<EmployeeDocument> EmployeeDocuments { get; set; } = new List<EmployeeDocument>();
}



