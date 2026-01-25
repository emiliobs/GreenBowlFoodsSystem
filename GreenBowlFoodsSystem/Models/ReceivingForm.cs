using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenBowlFoodsSystem.Models;

public class ReceivingForm
{
    public int Id { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime Date { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Select Supplier")]
    public int SupplierId { get; set; }

    [ForeignKey("SupplierId")]
    public Supplier? Supplier { get; set; } // e.g., "Raaz Food"

    [Required(ErrorMessage = "Trailer Number is required")]
    [StringLength(50)]
    [Display(Name = "Trailer #")]
    public string? TrailerNumber { get; set; } // e.g., "TR-5542"

    [Display(Name = "Accepted?")]
    public bool IsAccepted { get; set; } = true; // True = Stock accepted, False = Rejected due to quality

    [StringLength(500)]
    [Display(Name = "Inspection Notes")]
    public string? InspectionNotes { get; set; } // e.g. "Cleanliness OK"

    [Required(ErrorMessage = "Receiver is required")]
    public int ReceivedById { get; set; }

    [ForeignKey("ReceivedById")]
    [Display(Name = "Received By")]
    public User? ReceivedBy { get; set; }
}