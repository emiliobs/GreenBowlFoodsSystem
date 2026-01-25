using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenBowlFoodsSystem.Models;

public class PackagingMaterial
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Packaging Name is required")]
    [StringLength(100)]
    [Display(Name = "Material Name")]
    public string MaterialName { get; set; } = string.Empty; // e.g., "Box 20x20"

    [Range(0, 100000, ErrorMessage = "Quantity cannot be negative")]
    [Display(Name = "Qty In Stock (Units)")]
    public int QuantityInStock { get; set; }

    [Required(ErrorMessage = "Select a Supplier")]
    public int SupplierId { get; set; }

    [ForeignKey("SupplierId")]
    public Supplier? Supplier { get; set; }
}