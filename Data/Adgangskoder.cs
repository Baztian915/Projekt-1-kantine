using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

namespace Kantine.Data;

/// <summary>
/// Adgangskoder gemmes to gange:
///  - som hash, der bruges ved login (kan ikke laves tilbage til koden)
///  - krypteret, så admin kan se koden på siden Brugere
/// Krypteringsnøglen styres af ASP.NET Core Data Protection og ligger i Windows-brugerprofilen.
/// </summary>
public class Adgangskoder(IDataProtectionProvider provider)
{
    public const string AppNavn = "Kantine";

    private readonly IDataProtector _beskytter = provider.CreateProtector("Kantine.Adgangskoder");
    private readonly PasswordHasher<Bruger> _hasher = new();

    public string Hash(string kode) => _hasher.HashPassword(null!, kode);

    public bool ErKorrekt(Bruger bruger, string kode) =>
        _hasher.VerifyHashedPassword(bruger, bruger.KodeHash, kode) != PasswordVerificationResult.Failed;

    public string Krypter(string kode) => _beskytter.Protect(kode);

    /// <summary>Giver null, hvis koden ikke er gemt krypteret, eller nøglen ikke længere findes.</summary>
    public string? Dekrypter(string? krypteret)
    {
        if (krypteret is null)
            return null;
        try
        {
            return _beskytter.Unprotect(krypteret);
        }
        catch (CryptographicException)
        {
            return null;
        }
    }
}
