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
    [Display(Name = "Raw Material (Ingredient)")]
    public int RawMaterialId { get; set; }

    [ForeignKey("RawMaterialId")]
    [Display(Name = "Ingredient")]
    public RawMaterial? RawMaterial { get; set; }

    [Required]
    [Range(0.0001, 1000, ErrorMessage = "Quantity must be greater than 0")]
    [Column(TypeName = "decimal(18,4)")] // 4 decimals for precision (e.g. 0.005 kg of saffron)
    [Display(Name = "Quantity Required (per Unit)")]
    public decimal QuantityUsed { get; set; } // e.g., 0.25 (meaning 0.25 Kg per Salad)
}