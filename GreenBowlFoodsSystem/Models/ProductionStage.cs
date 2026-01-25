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
    [RegularExpression("^(Weighing|Mixing|Cooking|Cooling|Packaging)$", ErrorMessage = "Stage must be: Weighing, Mixing, Cooking, Cooling, or Packaging")]
    [Display(Name = "Stage Name")]
    public string StageName { get; set; } = string.Empty; // e.g., "Cooking", "Mixing", "Retorting"

    [Required]
    [Display(Name = "Start Time")]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
    public DateTime StartTime { get; set; } = DateTime.Now;

    [Display(Name = "End Time")]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
    public DateTime? EndTime { get; set; }

    [StringLength(50, ErrorMessage = "Temperature note too long")]
    [Display(Name = "Temp Check (e.g. 85C)")]
    public string? TemperatureCheck { get; set; } // e.g., "85°C" (Critical for food safety)

    [StringLength(500)]
    public string? Notes { get; set; } // e.g., "Added extra water due to viscosity"
}