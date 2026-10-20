using Kantine.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Kantine.Pages;

[Authorize(Roles = Roller.Kantine + "," + Roller.Admin)]
public class OversigtModel(Database database) : PageModel
{
    public List<DagsOptaelling> Dage { get; private set; } = [];

    public void OnGet()
    {
        Dage = database.HentOptaelling(Hverdage.Kommende());
    }
}
