using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenBowlFoodsSystem.Models;

public class Shipment
{
    public int Id { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime Date { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Carrier name is required")]
    [StringLength(50, ErrorMessage = "Carrier name too long")]
    public string? Carrier { get; set; } // e.g., "FedEx"

    [StringLength(50)]
    [Display(Name = "Tracking #")]
    public string? TrackingNumber { get; set; }

    [Required(ErrorMessage = "Select Customer")]
    public int CustomerId { get; set; }

    [ForeignKey("CustomerId")]
    public Customer? Customer { get; set; }

    [Required(ErrorMessage = "Vehicle Inspection (Delivery Form) is required")]
    public int DeliveryFormId { get; set; }

    [ForeignKey("DeliveryFormId")]
    [Display(Name = "Vehicle Check")]
    public DeliveryForm? DeliveryForm { get; set; }
}