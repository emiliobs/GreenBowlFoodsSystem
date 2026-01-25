using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenBowlFoodsSystem.Models;

public class DeliveryForm
{
    public int Id { get; set; }

    [Display(Name = "Check Date")]
    [DataType(DataType.Date)]
    public DateTime CheckDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Trailer Number is required")]
    [StringLength(50)]
    [Display(Name = "Trailer #")]
    public string? TrailerNumber { get; set; } // e.g., "DHL-99"; }

    [Display(Name = "Temperature OK?")]
    public bool IsTempOk { get; set; } // e.g., True if temp is < 4°C

    [Display(Name = "Trailer Clean?")]
    public bool IsClean { get; set; } // e.g., True if trailer is pest-free

    [Required(ErrorMessage = "Driver Name is required")]
    [StringLength(100)]
    [Display(Name = "Driver Name")]
    public string? DriverName { get; set; }

    [Required(ErrorMessage = "Approver is required")]
    public int ApprovedById { get; set; }

    [ForeignKey("ApprovedById")]
    [Display(Name = "Approved By")]
    public User? ApprovedBy { get; set; }
}