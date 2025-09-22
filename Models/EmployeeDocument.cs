using System.ComponentModel.DataAnnotations;

namespace EmployerApp.Api.Models;

public class EmployeeDocument
{
    public Guid Id { get; set; }

    [Required]
    public Guid EmployeeId { get; set; }

    [Required]
    public Guid DocumentTypeId { get; set; }

    [Required]
    [MaxLength(100)]
    public string DocumentName { get; set; } = string.Empty;

    [Required]
    public byte[] FileData { get; set; } = Array.Empty<byte>();

    [Required]
    public int FileSize { get; set; }

    [Required]
    [MaxLength(100)]
    public string MimeType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Employee Employee { get; set; } = null!;

    public virtual DocumentType DocumentType { get; set; } = null!;
}

