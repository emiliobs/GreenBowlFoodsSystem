using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // <--- Necesario para [NotMapped]

namespace GreenBowlFoodsSystem.Models
{
    // Heredamos de IdentityUser<int> para que el ID sea un número (1, 2, 3...)
    public class User : IdentityUser<int>
    {
        // New Personal Info
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(50)]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Role selection is required")]
        [RegularExpression("^(Admin|Staff)$", ErrorMessage = "Role must be 'Admin' or 'Staff'")]
        public string Role { get; set; } = "Staff";

        [NotMapped]
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";
    }
}