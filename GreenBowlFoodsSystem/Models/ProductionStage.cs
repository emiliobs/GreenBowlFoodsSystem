using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenBowlFoodsSystem.Models;

public class ProductionStage
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Batch is required")]
    public int ProductionBatchId { get; set; }

    [ForeignKey("ProductionBatchId")]
    public ProductionBatch? ProductionBatch { get; set; }

    [Required(ErrorMessage = "Stage Name is required")]
    [Display(Name = "Stage Name")]
    public StageType StageName { get; set; } // e.g., "Cooking", "Mixing", "Retorting"

    [Required]
    [Display(Name = "Start Time")]
    [DataType(DataType.DateTime)]
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    [Display(Name = "End Time")]
    [DataType(DataType.DateTime)]
    public DateTime? EndTime { get; set; }

    [Required]
    [Display(Name = "Temp (°C)")]
    [Range(-20, 200, ErrorMessage = "Enter a valid temperature (-20 to 200)")]
    public decimal? TemperatureCelsius { get; set; } // e.g., "85°C" (Critical for food safety)

    [StringLength(500)]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Observations / Notes")]
    public string? Notes { get; set; } // e.g., "Added extra water due to viscosity"
}