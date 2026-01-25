using System.ComponentModel.DataAnnotations;

namespace GreenBowlFoodsSystem.Models;

public class Customer
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Company Name is required")]
    [StringLength(100)]
    [Display(Name = "Company Name")]
    public string CustomerName { get; set; } = string.Empty; // e.g., "Costco Wholesale"

    [StringLength(100)]
    [Display(Name = "Contact Name")]
    public string? ContactName { get; set; } // e.g., "Sarah Smith"

    [Required(ErrorMessage = "Email Address is required for Invoicing")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Invalid Phone Number")]
    [Display(Name = "Phone Number")]
    public string? Phone { get; set; }

    [StringLength(200)]
    [Display(Name = "Billing Address")]
    public string? BillingAddress { get; set; }
}