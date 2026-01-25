using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenBowlFoodsSystem.Models;

public class InvoiceItem
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }

    [ForeignKey("InvoiceId")]
    public Invoice? Invoice { get; set; }

    [Required(ErrorMessage = "Please select a product")]
    public int FinishedProductId { get; set; }

    [ForeignKey("FinishedProductId")]
    [Display(Name = "Finished Product")]
    public FinishedProduct? FinishedProduct { get; set; } // e.g., "Quinopea"

    [Required]
    [Range(1, 10000, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; } // e.g., 200

    [Required]
    [DataType(DataType.Currency)]
    [Range(0.01, 10000, ErrorMessage = "Unit Price must be greater than 0")]
    [Display(Name = "Unit Price")]
    public decimal UnitPrice { get; set; } // e.g., 12.50
}