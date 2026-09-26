using System.ComponentModel.DataAnnotations;

namespace Nexodus_Back.Core.Entities;

public class Note
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    
    [Required]
    public string Type { get; set; } = string.Empty; // "idea" or "diario"
    
    public string? Title { get; set; }
    public string? Content { get; set; }
    
    public ICollection<ChecklistItem> Checklist { get; set; } = new List<ChecklistItem>();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
