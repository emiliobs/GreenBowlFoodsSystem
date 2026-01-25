using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenBowlFoodsSystem.Models;

public class ProductionMaterial
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Production Batch is required")]
    public int ProductionBatchId { get; set; }

    [ForeignKey("ProductionBatchId")]
    public ProductionBatch? ProductionBatch { get; set; }

    [Required(ErrorMessage = "Select an Ingredient")]
    public int RawMaterialId { get; set; }

    [ForeignKey("RawMaterialId")]
    [Display(Name = "Ingredient")]
    public RawMaterial? RawMaterial { get; set; }

    [Required]
    [Range(0.01, 10000, ErrorMessage = "Quantity used must be greater than 0")]
    [Display(Name = "Qty Used (Kg/L)")]
    public decimal QuantityUsed { get; set; } // e.g., 50.00 Kg, Deducted from stock
}