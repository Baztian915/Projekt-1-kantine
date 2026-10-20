using Microsoft.Data.Sqlite;

namespace Kantine.Data;

public enum Ret
{
    Vegansk = 1,
    Glutenfri = 2,
    Koed = 3
}

/// <summary>
/// Én dags valg for en medarbejder. GaesteRetter har én plads pr. gæst.
/// En ret er null, hvis den endnu ikke er valgt.
/// </summary>
public record DagsValg(bool Spiser, Ret? DinRet, IReadOnlyList<Ret?> GaesteRetter)
{
    public const int MaksGaester = 5;

    public static readonly DagsValg SpiserIkke = new(false, null, []);
}

/// <summary>KodeKrypteret er null for brugere oprettet, før koder blev gemt krypteret.</summary>
public record Bruger(int Id, string Brugernavn, string KodeHash, string? KodeKrypteret, string Rolle);

/// <summary>Antal for én dag. Rettene tæller portioner, dvs. både medarbejdere og gæster.</summary>
public record DagsOptaelling(DateOnly Dag, int Medarbejdere, int Gaester, int Vegansk, int Glutenfri, int Koed)
{
    public int Portioner => Medarbejdere + Gaester;

    /// <summary>Portioner fra tilmeldinger lavet før man skulle vælge ret.</summary>
    public int UdenRet => Portioner - Vegansk - Glutenfri - Koed;
}

public static class Roller
{
    public const string Medarbejder = "medarbejder";
    public const string Kantine = "kantine";
    public const string Admin = "admin";

    public static readonly string[] Alle = [Medarbejder, Kantine, Admin];

    public static string Visningsnavn(string rolle) => rolle switch
    {
        Medarbejder => "Medarbejder",
        Kantine => "Kantine",
        Admin => "Admin",
        _ => rolle
    };
}

/// <summary>
/// Al adgang til SQLite-databasen samles her, så SQL'en er nem at finde og læse.
/// Datoer gemmes som tekst i formatet yyyy-MM-dd.
/// </summary>
public class Database(string connectionString)
{
    private const string DatoFormat = "yyyy-MM-dd";

    private SqliteConnection Aabn()
    {
        var forbindelse = new SqliteConnection(connectionString);
        forbindelse.Open();
        return forbindelse;
    }

    public void OpretTabeller()
    {
        using var db = Aabn();
        using var cmd = db.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Brugere (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                Brugernavn  TEXT NOT NULL UNIQUE COLLATE NOCASE,
                KodeHash    TEXT NOT NULL,
                Rolle       TEXT NOT NULL CHECK (Rolle IN ('medarbejder', 'kantine', 'admin')),
                KodeKrypteret TEXT
            );

            -- Én række pr. person, der spiser: Nr 0 er medarbejderen selv, Nr 1, 2, ... er gæster
            CREATE TABLE IF NOT EXISTS Portioner (
                BrugerId    INTEGER NOT NULL REFERENCES Brugere(Id) ON DELETE CASCADE,
                Dato        TEXT NOT NULL,
                Nr          INTEGER NOT NULL CHECK (Nr >= 0),
                Ret         INTEGER CHECK (Ret IN (1, 2, 3)),
                PRIMARY KEY (BrugerId, Dato, Nr)
            );
            """;
        cmd.ExecuteNonQuery();

        TilladAdminRolle(db);
        TilfoejKrypteretKode(db);
        FlytTilmeldingerTilPortioner(db);
    }

    /// <summary>Databaser oprettet før admin kunne se adgangskoder, mangler kolonnen KodeKrypteret.</summary>
    private static void TilfoejKrypteretKode(SqliteConnection db)
    {
        using var tjek = db.CreateCommand();
        tjek.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Brugere') WHERE name = 'KodeKrypteret'";
        if ((long)tjek.ExecuteScalar()! > 0)
            return;

        using var cmd = db.CreateCommand();
        cmd.CommandText = "ALTER TABLE Brugere ADD COLUMN KodeKrypteret TEXT";
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Databaser oprettet før admin-rollen fandtes har en CHECK-regel uden 'admin'.
    /// SQLite kan ikke ændre en CHECK-regel, så tabellen bygges om med de samme brugere.
    /// </summary>
    private static void TilladAdminRolle(SqliteConnection db)
    {
        using var tjek = db.CreateCommand();
        tjek.CommandText = "SELECT sql FROM sqlite_master WHERE type = 'table' AND name = 'Brugere'";
        if (((string)tjek.ExecuteScalar()!).Contains("'admin'"))
            return;

        using var cmd = db.CreateCommand();
        cmd.CommandText = """
            PRAGMA foreign_keys = OFF;
            BEGIN;
            CREATE TABLE Brugere_ny (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                Brugernavn  TEXT NOT NULL UNIQUE COLLATE NOCASE,
                KodeHash    TEXT NOT NULL,
                Rolle       TEXT NOT NULL CHECK (Rolle IN ('medarbejder', 'kantine', 'admin'))
            );
            INSERT INTO Brugere_ny (Id, Brugernavn, KodeHash, Rolle) SELECT Id, Brugernavn, KodeHash, Rolle FROM Brugere;
            DROP TABLE Brugere;
            ALTER TABLE Brugere_ny RENAME TO Brugere;
            COMMIT;
            PRAGMA foreign_keys = ON;
            """;
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Ældre databaser gemte valget i tabellen Tilmeldinger (0 = spiser ikke, 1 = spiser, 2 = spiser + 1 gæst).
    /// De flyttes over i Portioner, hvor gæsten får samme ret som medarbejderen.
    /// </summary>
    private static void FlytTilmeldingerTilPortioner(SqliteConnection db)
    {
        using var tjek = db.CreateCommand();
        tjek.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'Tilmeldinger'";
        if ((long)tjek.ExecuteScalar()! == 0)
            return;

        // De allerældste databaser har ingen Ret-kolonne
        tjek.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Tilmeldinger') WHERE name = 'Ret'";
        var ret = (long)tjek.ExecuteScalar()! > 0 ? "Ret" : "NULL";

        using var cmd = db.CreateCommand();
        cmd.CommandText = $"""
            BEGIN;
            INSERT OR IGNORE INTO Portioner (BrugerId, Dato, Nr, Ret)
                SELECT BrugerId, Dato, 0, {ret} FROM Tilmeldinger WHERE Valg >= 1;
            INSERT OR IGNORE INTO Portioner (BrugerId, Dato, Nr, Ret)
                SELECT BrugerId, Dato, 1, {ret} FROM Tilmeldinger WHERE Valg = 2;
            DROP TABLE Tilmeldinger;
            COMMIT;
            """;
        cmd.ExecuteNonQuery();
    }

    private const string BrugerKolonner = "Id, Brugernavn, KodeHash, KodeKrypteret, Rolle";

    private static Bruger LaesBruger(SqliteDataReader r) =>
        new(r.GetInt32(0), r.GetString(1), r.GetString(2), r.IsDBNull(3) ? null : r.GetString(3), r.GetString(4));

    public Bruger? FindBruger(string brugernavn)
    {
        using var db = Aabn();
        using var cmd = db.CreateCommand();
        cmd.CommandText = $"SELECT {BrugerKolonner} FROM Brugere WHERE Brugernavn = $navn";
        cmd.Parameters.AddWithValue("$navn", brugernavn);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? LaesBruger(reader) : null;
    }

    public List<Bruger> HentBrugere()
    {
        var brugere = new List<Bruger>();

        using var db = Aabn();
        using var cmd = db.CreateCommand();
        cmd.CommandText = $"SELECT {BrugerKolonner} FROM Brugere ORDER BY Rolle, Brugernavn";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            brugere.Add(LaesBruger(reader));
        return brugere;
    }

    /// <summary>Giver false, hvis brugeren ikke findes.</summary>
    public bool SkiftKode(int id, string kodeHash, string kodeKrypteret)
    {
        using var db = Aabn();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "UPDATE Brugere SET KodeHash = $hash, KodeKrypteret = $krypteret WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.Parameters.AddWithValue("$hash", kodeHash);
        cmd.Parameters.AddWithValue("$krypteret", kodeKrypteret);
        return cmd.ExecuteNonQuery() > 0;
    }

    /// <summary>Sletter brugeren. Brugerens portioner slettes med (ON DELETE CASCADE).</summary>
    public void SletBruger(int id)
    {
        using var db = Aabn();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "DELETE FROM Brugere WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public void OpretBruger(string brugernavn, string kodeHash, string kodeKrypteret, string rolle)
    {
        using var db = Aabn();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "INSERT INTO Brugere (Brugernavn, KodeHash, KodeKrypteret, Rolle) VALUES ($navn, $hash, $krypteret, $rolle)";
        cmd.Parameters.AddWithValue("$navn", brugernavn);
        cmd.Parameters.AddWithValue("$hash", kodeHash);
        cmd.Parameters.AddWithValue("$krypteret", kodeKrypteret);
        cmd.Parameters.AddWithValue("$rolle", rolle);
        cmd.ExecuteNonQuery();
    }

    /// <summary>Henter brugerens valg for de givne dage. Dage uden portioner tæller som "spiser ikke".</summary>
    public Dictionary<DateOnly, DagsValg> HentTilmeldinger(int brugerId, IReadOnlyList<DateOnly> dage)
    {
        var personer = dage.ToDictionary(d => d, _ => new SortedDictionary<int, Ret?>());

        using var db = Aabn();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT Dato, Nr, Ret FROM Portioner WHERE BrugerId = $id AND Dato BETWEEN $fra AND $til";
        cmd.Parameters.AddWithValue("$id", brugerId);
        cmd.Parameters.AddWithValue("$fra", dage.Min().ToString(DatoFormat));
        cmd.Parameters.AddWithValue("$til", dage.Max().ToString(DatoFormat));

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var dag = DateOnly.ParseExact(reader.GetString(0), DatoFormat);
            if (personer.TryGetValue(dag, out var dagensPersoner))
                dagensPersoner[reader.GetInt32(1)] = reader.IsDBNull(2) ? null : (Ret)reader.GetInt32(2);
        }

        return personer.ToDictionary(
            p => p.Key,
            p => p.Value.TryGetValue(0, out var dinRet)
                ? new DagsValg(true, dinRet, p.Value.Where(x => x.Key > 0).Select(x => x.Value).ToList())
                : DagsValg.SpiserIkke);
    }

    /// <summary>Erstatter brugerens portioner for de givne dage med de nye valg.</summary>
    public void GemTilmeldinger(int brugerId, IReadOnlyDictionary<DateOnly, DagsValg> valg)
    {
        using var db = Aabn();
        using var transaktion = db.BeginTransaction();

        foreach (var (dag, dagsValg) in valg)
        {
            using var slet = db.CreateCommand();
            slet.CommandText = "DELETE FROM Portioner WHERE BrugerId = $id AND Dato = $dato";
            slet.Parameters.AddWithValue("$id", brugerId);
            slet.Parameters.AddWithValue("$dato", dag.ToString(DatoFormat));
            slet.ExecuteNonQuery();

            if (!dagsValg.Spiser)
                continue;

            // Medarbejderen selv er nr. 0, gæsterne nr. 1, 2, ...
            Ret?[] retter = [dagsValg.DinRet, .. dagsValg.GaesteRetter];
            for (var nr = 0; nr < retter.Length; nr++)
            {
                using var cmd = db.CreateCommand();
                cmd.CommandText = "INSERT INTO Portioner (BrugerId, Dato, Nr, Ret) VALUES ($id, $dato, $nr, $ret)";
                cmd.Parameters.AddWithValue("$id", brugerId);
                cmd.Parameters.AddWithValue("$dato", dag.ToString(DatoFormat));
                cmd.Parameters.AddWithValue("$nr", nr);
                cmd.Parameters.AddWithValue("$ret", retter[nr] is { } ret ? (int)ret : DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        transaktion.Commit();
    }

    public List<DagsOptaelling> HentOptaelling(IReadOnlyList<DateOnly> dage)
    {
        var tal = new Dictionary<DateOnly, DagsOptaelling>();

        using var db = Aabn();
        using var cmd = db.CreateCommand();
        cmd.CommandText = """
            SELECT Dato,
                   SUM(Nr = 0)  AS Medarbejdere,
                   SUM(Nr > 0)  AS Gaester,
                   SUM(Ret IS 1) AS Vegansk,
                   SUM(Ret IS 2) AS Glutenfri,
                   SUM(Ret IS 3) AS Koed
            FROM Portioner
            WHERE Dato BETWEEN $fra AND $til
            GROUP BY Dato
            """;
        cmd.Parameters.AddWithValue("$fra", dage.Min().ToString(DatoFormat));
        cmd.Parameters.AddWithValue("$til", dage.Max().ToString(DatoFormat));

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var dag = DateOnly.ParseExact(reader.GetString(0), DatoFormat);
            tal[dag] = new DagsOptaelling(dag, reader.GetInt32(1), reader.GetInt32(2),
                reader.GetInt32(3), reader.GetInt32(4), reader.GetInt32(5));
        }

        return dage.Select(d => tal.GetValueOrDefault(d) ?? new DagsOptaelling(d, 0, 0, 0, 0, 0)).ToList();
    }
}
