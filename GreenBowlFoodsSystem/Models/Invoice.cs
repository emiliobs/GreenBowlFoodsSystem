using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenBowlFoodsSystem.Models;

public class Invoice
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Invoice Number is required")]
    [StringLength(50)]
    [Display(Name = "Invoice #")]
    public string InvoiceNumber { get; set; } = string.Empty; // e.g., "INV-2026-001"

    [Required(ErrorMessage = "Customer is required")]
    public int CustomerId { get; set; }

    [ForeignKey("CustomerId")]
    public Customer? Customer { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime Date { get; set; } = DateTime.Now;

    [DataType(DataType.Currency)]
    [Range(0, 1000000, ErrorMessage = "Total cannot be negative")]
    [Display(Name = "Total Amount")]
    public decimal TotalAmount { get; set; } // e.g., 2500.00

    [Required]
    [RegularExpression("^(Paid|Unpaid|Overdue)$", ErrorMessage = "Status must be: Paid, Unpaid, or Overdue")]
    public string Status { get; set; } = "Unpaid";

    public List<InvoiceItem> Items { get; set; } = new();
}