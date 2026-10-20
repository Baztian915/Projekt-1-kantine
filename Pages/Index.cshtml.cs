using Kantine.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Kantine.Pages;

/// <summary>Forsiden sender brugeren videre til den side, der passer til rollen.</summary>
public class IndexModel : PageModel
{
    public IActionResult OnGet() =>
        User.IsInRole(Roller.Kantine) || User.IsInRole(Roller.Admin) ? RedirectToPage("/Oversigt") : RedirectToPage("/Valg");
}
