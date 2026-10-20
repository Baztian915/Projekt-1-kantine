using System.Security.Claims;
using Kantine.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Kantine.Pages;

[Authorize(Roles = Roller.Admin)]
public class BrugereModel(Database database, Adgangskoder adgangskoder) : PageModel
{
    [BindProperty] public string? Brugernavn { get; set; }
    [BindProperty] public string? Kode { get; set; }
    [BindProperty] public string Rolle { get; set; } = Roller.Medarbejder;

    public List<Bruger> Brugere { get; private set; } = [];
    public string? Fejl { get; private set; }

    public int MitId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Brugerens nuværende kode i klartekst, eller null hvis den ikke kan vises.</summary>
    public string? VisKode(Bruger bruger) => adgangskoder.Dekrypter(bruger.KodeKrypteret);

    public void OnGet()
    {
        Brugere = database.HentBrugere();
    }

    public IActionResult OnPostOpret()
    {
        Brugernavn = Brugernavn?.Trim();

        if (string.IsNullOrEmpty(Brugernavn) || string.IsNullOrEmpty(Kode))
            Fejl = "Udfyld både brugernavn og adgangskode.";
        else if (!Roller.Alle.Contains(Rolle))
            Fejl = "Vælg en gyldig rolle.";
        else if (database.FindBruger(Brugernavn) is not null)
            Fejl = $"Brugernavnet '{Brugernavn}' er allerede i brug.";

        if (Fejl is not null)
        {
            Brugere = database.HentBrugere();
            return Page();
        }

        database.OpretBruger(Brugernavn!, adgangskoder.Hash(Kode!), adgangskoder.Krypter(Kode!), Rolle);
        TempData["Besked"] = $"Brugeren '{Brugernavn}' er oprettet.";
        return RedirectToPage();
    }

    public IActionResult OnPostSkiftKode(int id, string navn, string? nyKode)
    {
        if (string.IsNullOrEmpty(nyKode))
        {
            TempData["Besked"] = $"Skriv en ny adgangskode til '{navn}'.";
            return RedirectToPage();
        }

        TempData["Besked"] = database.SkiftKode(id, adgangskoder.Hash(nyKode), adgangskoder.Krypter(nyKode))
            ? $"Adgangskoden til '{navn}' er ændret."
            : $"Brugeren '{navn}' findes ikke længere.";
        return RedirectToPage();
    }

    public IActionResult OnPostSlet(int id, string navn)
    {
        // Admin må ikke kunne låse sig selv ude
        if (id == MitId)
        {
            TempData["Besked"] = "Du kan ikke slette din egen bruger.";
            return RedirectToPage();
        }

        database.SletBruger(id);
        TempData["Besked"] = $"Brugeren '{navn}' er slettet.";
        return RedirectToPage();
    }
}
