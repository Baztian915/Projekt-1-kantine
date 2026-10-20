using System.Security.Claims;
using Kantine.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Kantine.Pages;

[Authorize(Roles = Roller.Medarbejder + "," + Roller.Admin)]
public class ValgModel(Database database) : PageModel
{
    // Formularens felter. Nøglen er datoen (yyyy-MM-dd), for gæsternes ret dato + "_" + gæstens nummer.
    [BindProperty] public Dictionary<string, bool> Spiser { get; set; } = [];
    [BindProperty] public Dictionary<string, Ret> Ret { get; set; } = [];
    [BindProperty] public Dictionary<string, int> Gaester { get; set; } = [];
    [BindProperty] public Dictionary<string, Ret> GaestRet { get; set; } = [];

    public Dictionary<DateOnly, DagsValg> Tilmeldinger { get; private set; } = [];
    public string? Fejl { get; private set; }

    private int BrugerId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public void OnGet()
    {
        Tilmeldinger = database.HentTilmeldinger(BrugerId, Hverdage.Kommende());
    }

    public IActionResult OnPost()
    {
        // Brug kun dage inden for de kommende hverdage – alt andet i formularen ignoreres
        var valg = new Dictionary<DateOnly, DagsValg>();
        var manglerRet = new List<DateOnly>();

        foreach (var dag in Hverdage.Kommende())
        {
            var noegle = dag.ToString("yyyy-MM-dd");

            if (!Spiser.GetValueOrDefault(noegle))
            {
                valg[dag] = DagsValg.SpiserIkke;
                continue;
            }

            var antalGaester = Math.Clamp(Gaester.GetValueOrDefault(noegle), 0, DagsValg.MaksGaester);
            var dinRet = GyldigRet(Ret, noegle);
            var gaesteRetter = Enumerable.Range(1, antalGaester).Select(nr => GyldigRet(GaestRet, $"{noegle}_{nr}")).ToList();

            if (dinRet is null || gaesteRetter.Contains(null))
                manglerRet.Add(dag);

            valg[dag] = new DagsValg(true, dinRet, gaesteRetter);
        }

        if (manglerRet.Count > 0)
        {
            Fejl = "Vælg en ret til alle personer for " + string.Join(", ", manglerRet.Select(d => Hverdage.Ugedag(d).ToLower())) + ".";
            Tilmeldinger = valg;
            return Page();
        }

        database.GemTilmeldinger(BrugerId, valg);
        TempData["Besked"] = "Dine valg er gemt.";
        return RedirectToPage();
    }

    private static Ret? GyldigRet(Dictionary<string, Ret> retter, string noegle) =>
        retter.TryGetValue(noegle, out var ret) && Enum.IsDefined(ret) ? ret : null;
}
