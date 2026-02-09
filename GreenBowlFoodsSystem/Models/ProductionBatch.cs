using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenBowlFoodsSystem.Models;

public class ProductionBatch
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Batch Number is mandatory")]
    [StringLength(50, ErrorMessage = "Batch Number too long")]
    [Display(Name = "Batch #")]
    public string BatchNumber { get; set; } = string.Empty; // e.g., "BATCH-2026-A"

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Started At")]
    public DateTime ProductionDate { get; set; } = DateTime.Now;

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Completed At")]
    public DateTime? EndDate { get; set; } // Nullable: Empty until we click "Finish"

    [Required(ErrorMessage = "Select the Product being produced")]
    public int FinishedProductId { get; set; }

    [ForeignKey("FinishedProductId")]
    [Display(Name = "Finished Product")]
    public FinishedProduct? FinishedProduct { get; set; } // e.g., Making "Quinopea"

    [Required(ErrorMessage = "Supervisor is required")]
    public int SupervisorId { get; set; }

    [ForeignKey("SupervisorId")]
    public User? Supervisor { get; set; }

    // Efficiency KPIs
    [Display(Name = "Target Qty")]
    public int TargetQuantity { get; set; } // Planned Output

    [Required]
    [Range(0, 100000, ErrorMessage = "Target Quantity must be positive")]
    [Display(Name = "Quantity Produced")]
    public int QuantityProduced { get; set; } // Actual Output

    [Range(0, 1440, ErrorMessage = "Downtime cannot be negative")] // Max 1440 mins in a day
    [Display(Name = "Downtime (Mins)")]
    public int DowntimeMinutes { get; set; } // Machine breakdown time

    [Required]
    [RegularExpression("^(Planned|In Progress|Completed|QA Hold|Cancelled)$", ErrorMessage = "Invalid Status")]
    public string Status { get; set; } = "Planned"; // "Planned", "In Progress", "Completed", "QA Hold"

    // This tells Entity Framework: "A Production Batch can have MANY stages"
    public List<ProductionStage> ProductionStages { get; set; } = new List<ProductionStage>(); // E.g., "Mixing", "Cooking", "Packaging"

    // Thi talls Entity Framework: "A Production Batch can have MANY ingredients used"
    public List<ProductionMaterial>? ProductionMaterials { get; set; } = new List<ProductionMaterial>(); // E.g., "100kg Quinoa", "50L Vegetable Broth"
}