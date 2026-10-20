using Microsoft.AspNetCore.Html;

namespace Kantine;

/// <summary>Små streg-ikoner (SVG), der arver tekstfarven, hvor de bruges.</summary>
public static class Ikon
{
    private static HtmlString Svg(string indhold) => new(
        $"""<svg class="ikon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">{indhold}</svg>""");

    public static readonly HtmlString Bestik = Svg("""<path d="M3 2v7c0 1.1.9 2 2 2h4a2 2 0 0 0 2-2V2M7 2v20M21 15V2a5 5 0 0 0-5 5v6c0 1.1.9 2 2 2h3Zm0 0v7"/>""");
    public static readonly HtmlString Kalender = Svg("""<rect x="3" y="4" width="18" height="18" rx="2"/><path d="M16 2v4M8 2v4M3 10h18"/>""");
    public static readonly HtmlString Diagram = Svg("""<path d="M3 3v18h18"/><path d="M8 17v-4M13 17V9M18 17V5"/>""");
    public static readonly HtmlString Brugere = Svg("""<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75"/>""");
    public static readonly HtmlString NyBruger = Svg("""<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M19 8v6M22 11h-6"/>""");
    public static readonly HtmlString LogUd = Svg("""<path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><path d="m16 17 5-5-5-5M21 12H9"/>""");
    public static readonly HtmlString Kryds = Svg("""<path d="M18 6 6 18M6 6l12 12"/>""");
    public static readonly HtmlString Flueben = Svg("""<path d="M20 6 9 17l-5-5"/>""");
    public static readonly HtmlString Skraldespand = Svg("""<path d="M3 6h18M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/>""");
    public static readonly HtmlString Noegle = Svg("""<circle cx="7.5" cy="15.5" r="5.5"/><path d="m21 2-9.6 9.6M15.5 7.5l3 3L22 7l-3-3"/>""");
    public static readonly HtmlString Oeje = Svg("""<path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z"/><circle cx="12" cy="12" r="3"/>""");
    public static readonly HtmlString Advarsel = Svg("""<circle cx="12" cy="12" r="10"/><path d="M12 8v4M12 16h.01"/>""");
}
