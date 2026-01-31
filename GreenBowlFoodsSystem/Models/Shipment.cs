using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenBowlFoodsSystem.Models
{
    public class Shipment
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Shipment Date")]
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

        // Requirement: Link to Delivery Form (Vehicle Check).
        // Made optional temporarily so we can create shipments before the Delivery module is fully built.
        public int? DeliveryFormId { get; set; }

        [ForeignKey("DeliveryFormId")]
        [Display(Name = "Vehicle Check")]
        public DeliveryForm? DeliveryForm { get; set; }

        // Without these fields, we cannot deduct stock from the warehouse.
        [Required(ErrorMessage = "Select the product to ship")]
        public int FinishedProductId { get; set; }

        [ForeignKey("FinishedProductId")]
        [Display(Name = "Product")]
        public FinishedProduct? FinishedProduct { get; set; }

        [Required]
        [Range(1, 100000, ErrorMessage = "Quantity must be greater than 0")]
        [Display(Name = "Qty Shipped")]
        public int QuantityShipped { get; set; }

        // Stores the monetary value of the shipment for "Sales for a specific period" reports.
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "£{0:N2}", ApplyFormatInEditMode = false)]
        [Display(Name = "Total Value")]
        public decimal TotalValue { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";
    }
}