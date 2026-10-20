namespace Kantine.Data;

public static class Hverdage
{
    public const int Antal = 5;

    /// <summary>
    /// De næste 5 hverdage (mandag-fredag), med dags dato som den første hvis det er en hverdag.
    /// Helligdage tages der ikke højde for.
    /// </summary>
    public static List<DateOnly> Kommende()
    {
        var dage = new List<DateOnly>();
        var dag = DateOnly.FromDateTime(DateTime.Today);

        while (dage.Count < Antal)
        {
            if (dag.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
                dage.Add(dag);
            dag = dag.AddDays(1);
        }
        return dage;
    }

    /// <summary>F.eks. "Tirsdag".</summary>
    public static string Ugedag(DateOnly dag) => StortBegyndelsesbogstav(dag.ToString("dddd"));

    /// <summary>F.eks. "20. oktober".</summary>
    public static string Dato(DateOnly dag) => dag.ToString("d. MMMM");

    private static string StortBegyndelsesbogstav(string tekst) => char.ToUpper(tekst[0]) + tekst[1..];
}
