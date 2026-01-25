using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GreenBowlFoodsSystem.Models;

// 1. USER (Login y Auditoría)
public class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [Display(Name = "User Name")]
    public string Username { get; set; } = string.Empty; // e.g., "j.smith"

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty; // Stored plain for prototype

    [Required(ErrorMessage = "Role selection is required")]
    [RegularExpression("^(Admin|Staff)$", ErrorMessage = "Role must be 'Admin' or 'Staff'")]
    public string Role { get; set; } = "Staff"; // Security Level
}