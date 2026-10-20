# Guide: Upload Kantinen til GitHub

Denne guide viser trin for trin, hvordan hele projektet kommer op på GitHub.
Du skal kun vælge **én** af de tre metoder – **Metode A (GitHub Desktop)** er den nemmeste.

---

## 📁 Her ligger det hele

| Hvad | Placering |
|---|---|
| **Projektet** (det du arbejder i) | `C:\Users\BS\Documents\Smartlearning\Programmering\Projekt 1 kantine` |
| **Upload-klar samling** (kun de filer der skal op – til Metode B) | `C:\Users\BS\Documents\Smartlearning\Programmering\Kantine-til-GitHub` |
| Projektbeskrivelse (vises på forsiden af GitHub) | `README.md` i projektmappen |
| Liste over filer der **ikke** må uploades | `.gitignore` i projektmappen |
| Skærmbilleder til README | `docs\skaermbilleder\` i projektmappen |
| Din database med brugere (uploades **ikke**) | `kantine.db` i projektmappen |
| Dit projekt på GitHub, når det er uploadet | `https://github.com/<dit-brugernavn>/Kantine` |

---

## ✅ Hvad skal med – og hvad må IKKE med

**Skal med:** al kildekode (`Program.cs`, `Ikon.cs`, `Kantine.csproj`, `appsettings*.json`, mapperne `Data`, `Pages`,
`Properties`, `wwwroot`, `docs`) samt `README.md`, `GITHUB-GUIDE.md` og `.gitignore`.

**Må IKKE med:**

| Fil/mappe | Hvorfor |
|---|---|
| `kantine.db` | ⚠️ Indeholder alle brugere og deres adgangskoder |
| `bin\` og `obj\` | Byggefiler – laves automatisk igen med `dotnet run` |

Filen `.gitignore` sørger for, at disse automatisk bliver sprunget over i Metode A og C.
I Metode B bruger du den upload-klare samling, hvor de allerede er fjernet.

---

## Før du starter

1. Opret en gratis konto på [github.com](https://github.com/signup), hvis du ikke har en.
2. **Luk appen**, hvis den kører (tryk `Ctrl+C` i terminalen, hvor du skrev `dotnet run`).

---

## Metode A: GitHub Desktop (anbefalet)

GitHub Desktop er et program med knapper i stedet for kommandoer. Git er indbygget, så du skal ikke installere andet.

### 1. Installér og log ind
1. Hent programmet på [desktop.github.com](https://desktop.github.com) og installér det.
2. Åbn det og vælg **Sign in to GitHub.com**. Log ind i browseren og godkend.
3. Når den spørger om navn og e-mail til "Git", så tryk **Finish** (det udfyldes fra din GitHub-konto).

### 2. Gør projektmappen til et repository
1. Vælg menuen **File → Add local repository…**
2. Tryk **Choose…** og vælg mappen
   `C:\Users\BS\Documents\Smartlearning\Programmering\Projekt 1 kantine`
3. Der står nu *"This directory does not appear to be a Git repository"*. Tryk på linket **create a repository**.
4. Udfyld:
   - **Name:** `Kantine`
   - **Git ignore:** `None` (projektet har allerede sin egen `.gitignore`)
   - **License:** `None`
   - Lad **Initialize this repository with a README** være **slået fra** (der er allerede en README)
5. Tryk **Create repository**.

### 3. Tjek listen over filer
Til venstre under **Changes** ser du alle filer, der kommer med.
Kontrollér at der **ikke** står `kantine.db`, `bin` eller `obj` på listen.
(Hvis de står der, er `.gitignore` ikke kommet med – se *Fejlfinding* nederst.)

### 4. Gem den første version (commit)
1. Nederst til venstre i feltet **Summary** skriver du fx: `Første version af Kantinen`
2. Tryk **Commit to main**.

### 5. Upload til GitHub (publish)
1. Tryk på den blå knap **Publish repository** øverst.
2. **Name:** `Kantine`
3. **Keep this code private:**
   - Slå til = kun du (og dem du inviterer, fx din underviser) kan se koden.
   - Slå fra = alle kan se koden.
4. Tryk **Publish repository**.

Færdig! 🎉 Vælg **Repository → View on GitHub** for at se projektet i browseren.

### Når du senere ændrer noget
1. Åbn GitHub Desktop – ændringerne står automatisk under **Changes**.
2. Skriv en kort beskrivelse i **Summary** (fx `Tilføjet helligdage`) og tryk **Commit to main**.
3. Tryk **Push origin** øverst.

---

## Metode B: Upload direkte i browseren

Kræver ingen installation, men du skal selv uploade igen, hver gang du ændrer noget.

### 1. Opret et tomt repository
1. Gå til [github.com/new](https://github.com/new).
2. **Repository name:** `Kantine`
3. Vælg **Public** eller **Private**.
4. Sæt **ikke** flueben i *Add a README file*, *.gitignore* eller *license* – de findes allerede.
5. Tryk **Create repository**.

### 2. Upload filerne
1. På den nye side, tryk på linket **uploading an existing file**.
2. Åbn i Stifinder den upload-klare samling:
   `C:\Users\BS\Documents\Smartlearning\Programmering\Kantine-til-GitHub`
3. Markér **alt indholdet i mappen** (`Ctrl+A`) – ikke selve mappen – og træk det over i browservinduet.
   Mapperne (`Data`, `Pages` osv.) kommer med, når du bruger Edge eller Chrome.
4. Nederst: skriv `Første version af Kantinen` og tryk **Commit changes**.

> **Bemærk:** `.gitignore` er en "skjult" fil og kan mangle, hvis Stifinder skjuler den.
> Tjek bagefter, at den er på GitHub. Mangler den, så vælg **Add file → Upload files** og upload den alene.

### Opdatér den upload-klare samling
Samlingen er et øjebliksbillede. Har du ændret i projektet, så kør denne kommando i PowerShell for at opdatere den,
og upload derefter igen (GitHub erstatter filer med samme navn):

```powershell
robocopy "C:\Users\BS\Documents\Smartlearning\Programmering\Projekt 1 kantine" "C:\Users\BS\Documents\Smartlearning\Programmering\Kantine-til-GitHub" /MIR /XD bin obj .vs /XF *.db *.db-shm *.db-wal
```

---

## Metode C: Git på kommandolinjen

Til dig, der vil lære git-kommandoerne.

1. Installér Git fra [git-scm.com/download/win](https://git-scm.com/download/win) (standardvalgene er fine). Genstart terminalen bagefter.
2. Fortæl git hvem du er (kun første gang):
   ```powershell
   git config --global user.name "Dit Navn"
   git config --global user.email "din@email.dk"
   ```
3. Opret et tomt repository på [github.com/new](https://github.com/new) som i Metode B, trin 1.
4. Kør i projektmappen:
   ```powershell
   cd "C:\Users\BS\Documents\Smartlearning\Programmering\Projekt 1 kantine"
   git init -b main
   git add .
   git status            # tjek at kantine.db, bin og obj IKKE står på listen
   git commit -m "Første version af Kantinen"
   git remote add origin https://github.com/<dit-brugernavn>/Kantine.git
   git push -u origin main
   ```
   Første gang åbner et vindue, hvor du logger ind på GitHub.
5. Senere ændringer:
   ```powershell
   git add .
   git commit -m "Beskriv ændringen"
   git push
   ```

---

## 🔍 Tjek at alt er kommet rigtigt op

Åbn `https://github.com/<dit-brugernavn>/Kantine` og kontrollér:

- [ ] Mapperne `Data`, `Pages`, `Properties`, `wwwroot` og `docs` er der
- [ ] Filerne `Program.cs`, `Ikon.cs`, `Kantine.csproj`, `README.md`, `GITHUB-GUIDE.md` og `.gitignore` er der
- [ ] README vises under fillisten med skærmbilleder
- [ ] **`kantine.db` er IKKE der**, og heller ikke `bin` eller `obj`

---

## 💻 Hent og kør projektet på en anden computer

1. På GitHub: tryk den grønne knap **Code → Download ZIP** (eller **Open with GitHub Desktop**), og pak ud.
2. Installér [.NET 10 SDK](https://dotnet.microsoft.com/download), hvis den ikke er der.
3. Åbn en terminal i mappen og kør:
   ```powershell
   dotnet run -- opret-bruger admin <vælg-en-kode> admin
   dotnet run
   ```
4. Åbn http://localhost:5138 og log ind som admin.

Databasen følger ikke med, så man starter altid med en tom database og opretter selv sin admin-bruger.

---

## 🛠️ Fejlfinding

**`kantine.db` eller `bin`/`obj` står på listen i GitHub Desktop**
Tjek at filen `.gitignore` ligger i projektmappen (den kan være skjult i Stifinder – slå *Vis → Skjulte elementer* til).

**Jeg kom til at uploade `kantine.db`**
Slet den på GitHub (åbn filen → de tre prikker `…` → **Delete file** → **Commit changes**).
Filen kan stadig findes i historikken, så **skift alle adgangskoder** bagefter på siden **Brugere**.

**"Publish repository" fejler med at navnet er taget**
Du har allerede et repository med det navn på GitHub. Vælg et andet navn, fx `Kantine-projekt`.

**Windows blokerer appen ("En politik for programkontrol har blokeret denne fil")**
Det er Windows' Smart App Control, der kan blokere nybyggede filer. Kør `dotnet run` igen – det virker som regel anden gang.
