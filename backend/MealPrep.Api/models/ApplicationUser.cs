using Microsoft.AspNetCore.Identity;

namespace MealPrep.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
}
