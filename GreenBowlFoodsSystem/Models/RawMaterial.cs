using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenBowlFoodsSystem.Models;

public class RawMaterial
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Material Name is required")]
    [StringLength(100)]
    [Display(Name = "Material Name")]
    public string MaterialName { get; set; } = string.Empty; // e.g., "Basmati Rice"

    [Required(ErrorMessage = "Lot Number is required for Traceability")]
    [StringLength(50)]
    [Display(Name = "Lot Number")]
    public string LotNumber { get; set; } = string.Empty; // e.g., "L-8821"

    [Range(0, 100000, ErrorMessage = "Quantity must be positive")]
    [Display(Name = "Qty In Stock")]
    public decimal QuantityInStock { get; set; } // e.g., 500.00

    [Required]
    [StringLength(20)]
    public string Unit { get; set; } = "Kg"; // e.g., "Kg", "Liters", "Units"

    [Required(ErrorMessage = "Expiry Date is required")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Expiry Date")]
    public DateTime ExpiryDate { get; set; } // Critical for FIFO (First-In-First-Out)

    [Required(ErrorMessage = "Please select a Supplier")]
    public int SupplierId { get; set; }

    [ForeignKey("SupplierId")]
    public Supplier? Supplier { get; set; }
}