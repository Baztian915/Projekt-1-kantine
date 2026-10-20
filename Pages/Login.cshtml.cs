using System.Security.Claims;
using Kantine.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Kantine.Pages;

public class LoginModel(Database database, Adgangskoder adgangskoder) : PageModel
{
    [BindProperty] public string? Brugernavn { get; set; }
    [BindProperty] public string? Kode { get; set; }
    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

    public string? Fejl { get; private set; }

    public IActionResult OnGet() =>
        User.Identity?.IsAuthenticated == true ? RedirectToPage("/Index") : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        var bruger = string.IsNullOrWhiteSpace(Brugernavn) ? null : database.FindBruger(Brugernavn.Trim());

        if (bruger is null || string.IsNullOrEmpty(Kode) || !adgangskoder.ErKorrekt(bruger, Kode))
        {
            Fejl = "Forkert brugernavn eller adgangskode.";
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, bruger.Id.ToString()),
            new(ClaimTypes.Name, bruger.Brugernavn),
            new(ClaimTypes.Role, bruger.Rolle)
        };
        var identitet = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(new ClaimsPrincipal(identitet), new AuthenticationProperties { IsPersistent = true });

        // Kun lokale adresser, så login-siden ikke kan bruges til at sende folk videre til fremmede sider
        return Url.IsLocalUrl(ReturnUrl) ? LocalRedirect(ReturnUrl) : RedirectToPage("/Index");
    }
}
