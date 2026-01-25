using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenBowlFoodsSystem.Models;

public class XRayCheck
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Batch selection is required")]
    public int ProductionBatchId { get; set; }

    [ForeignKey("ProductionBatchId")]
    public ProductionBatch? ProductionBatch { get; set; }

    [Display(Name = "Check Time")]
    [DataType(DataType.DateTime)]
    public DateTime CheckTime { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Result is required")]
    [RegularExpression("^(Pass|Fail)$", ErrorMessage = "Result must be 'Pass' or 'Fail'")]
    public string Result { get; set; } = "Pass"; // "Pass" or "Fail"

    [StringLength(500, ErrorMessage = "Comment is too long")]
    public string? Comments { get; set; } // e.g., "Small metal fragment detected, rejected."

    [Display(Name = "Operator")]
    [Required(ErrorMessage = "Operator is required")]
    public int OperatorId { get; set; }

    [ForeignKey("OperatorId")]
    public User? Operator { get; set; }
}