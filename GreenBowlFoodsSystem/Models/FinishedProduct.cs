using System.ComponentModel.DataAnnotations;

namespace GreenBowlFoodsSystem.Models;

public class FinishedProduct
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Product Name is required")]
    [StringLength(100)]
    [Display(Name = "Product Name")]
    public string ProductName { get; set; } = string.Empty; // e.g., "Quinopea"

    [Required(ErrorMessage = "SKU is required")]
    [StringLength(50)]
    [Display(Name = "SKU (Stock Keeping Unit)")]
    public string SKU { get; set; } = string.Empty; // e.g., "QP-001";"Stock Keeping Unit" (Unidad de Mantenimiento de Existencias)

    [Range(0, 1000000, ErrorMessage = "Stock cannot be negative")]
    [Display(Name = "Quantity Available")]
    public int QuantityAvailable { get; set; }

    [Required]
    [DataType(DataType.Currency)]
    [Range(0.01, 10000, ErrorMessage = "Price must be greater than 0")]
    [Display(Name = "Unit Price")]
    public decimal UnitPrice { get; set; } // e.g., 12.50
}