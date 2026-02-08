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
    public Supplier? Supplier { get; set; }

    [Required(ErrorMessage = "Trailer Number is required")]
    [StringLength(50)]
    [Display(Name = "Trailer #")]
    public string? TrailerNumber { get; set; }

    [Display(Name = "Accepted?")]
    public bool IsAccepted { get; set; } = true;

    [StringLength(500)]
    [Display(Name = "Inspection Notes")]
    public string? InspectionNotes { get; set; }

    [DisplayFormat(DataFormatString = "£{0:N2}", ApplyFormatInEditMode = false)]
    [Range(0, 999999.99)]
    [DataType(DataType.Currency)]
    [Display(Name = "Total Cost")]
    public decimal TotalAmount { get; set; } // <--- ¡AQUÍ ESTÁ DE VUELTA!

    [Required(ErrorMessage = "Receiver is required")]
    public int ReceivedById { get; set; }

    [ForeignKey("ReceivedById")]
    [Display(Name = "Received By")]
    public User? ReceivedBy { get; set; }

    [Required]
    [Display(Name = "Raw Material")]
    public int RawMaterialId { get; set; }

    [ForeignKey("RawMaterialId")]
    public RawMaterial? RawMaterial { get; set; }

    [Required]
    [Range(0.01, 10000)]
    [Display(Name = "Qty Received")]
    public decimal QuantityReceived { get; set; }
}