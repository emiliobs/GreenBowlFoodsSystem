using System.ComponentModel.DataAnnotations;

namespace GreenBowlFoodsSystem.Models;

public class Supplier
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Supplier Name is required")]
    [StringLength(100)]
    [Display(Name = "Supplier Name")]
    public string SupplierName { get; set; } = string.Empty; // e.g., "Raaz Food Ingredients"

    [Display(Name = "Contact Person")]
    [StringLength(100)]
    public string? ContactPerson { get; set; }

    [Phone(ErrorMessage = "Invalid Phone Number")]
    [Display(Name = "Phone Number")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Email is required for Purchase Orders")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string? Email { get; set; }
}