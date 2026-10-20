using System.Globalization;
using System.Security.Claims;
using Kantine.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

// Danske ugedage og månedsnavne overalt i appen
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("da-DK");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("da-DK");

var builder = WebApplication.CreateBuilder(args);

var database = new Database(builder.Configuration.GetConnectionString("Kantine")!);
database.OpretTabeller();

// Kommandolinje-værktøj til at lægge brugere ind, f.eks.:
//   dotnet run -- opret-bruger anna hemmeligKode medarbejder
//   dotnet run -- hash-kode hemmeligKode
if (args.Length > 0 && (args[0] == "opret-bruger" || args[0] == "hash-kode"))
{
    // Samme appnavn som hjemmesiden, så koder krypteret her kan vises på siden Brugere
    var adgangskoder = new Adgangskoder(DataProtectionProvider.Create(Adgangskoder.AppNavn));
    Environment.ExitCode = Brugerværktøj.Kør(args, database, adgangskoder);
    return;
}

builder.Services.AddDataProtection().SetApplicationName(Adgangskoder.AppNavn);
builder.Services.AddSingleton(database);
builder.Services.AddSingleton<Adgangskoder>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AdgangNaegtet";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;

        // Log brugeren ud, hvis admin har slettet brugeren, mens de var logget ind
        options.Events.OnValidatePrincipal = async context =>
        {
            var bruger = database.FindBruger(context.Principal!.Identity!.Name!);
            var id = context.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (bruger is null || bruger.Id.ToString() != id)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync();
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddRazorPages(options =>
{
    // Alt kræver login, undtagen selve login-siden
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Login");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();

static class Brugerværktøj
{
    public static int Kør(string[] args, Database database, Adgangskoder adgangskoder)
    {
        if (args[0] == "hash-kode" && args.Length == 2)
        {
            Console.WriteLine(adgangskoder.Hash(args[1]));
            return 0;
        }

        if (args[0] == "opret-bruger" && args.Length == 4)
        {
            var (brugernavn, kode, rolle) = (args[1], args[2], args[3].ToLowerInvariant());

            if (!Roller.Alle.Contains(rolle))
            {
                Console.Error.WriteLine($"Ukendt rolle '{rolle}'. Brug en af: {string.Join(", ", Roller.Alle)}");
                return 1;
            }
            if (database.FindBruger(brugernavn) is not null)
            {
                Console.Error.WriteLine($"Brugeren '{brugernavn}' findes allerede.");
                return 1;
            }

            database.OpretBruger(brugernavn, adgangskoder.Hash(kode), adgangskoder.Krypter(kode), rolle);
            Console.WriteLine($"Oprettede brugeren '{brugernavn}' med rollen '{rolle}'.");
            return 0;
        }

        Console.Error.WriteLine("""
            Brug:
              dotnet run -- opret-bruger <brugernavn> <adgangskode> <medarbejder|kantine|admin>
              dotnet run -- hash-kode <adgangskode>
            """);
        return 1;
    }
}
