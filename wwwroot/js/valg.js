// Viser ret- og gæstevalg på de dage man spiser, og fortæller om der mangler noget eller ikke er gemt
const formular = document.getElementById("valg-formular");
const status = document.getElementById("gem-status");
const gemBar = formular.querySelector(".gem-bar");
const dage = [...formular.querySelectorAll(".dagsraekke")];

const tilstand = () => new URLSearchParams(new FormData(formular)).toString();
const gemt = tilstand();
let sender = false;

const spiser = dag => dag.querySelector(".segment input:checked")?.value === "true";

// Personer (dig + gæster), der skal have valgt en ret
const synligePersoner = dag => spiser(dag) ? [...dag.querySelectorAll(".person")].filter(p => !p.hidden) : [];

const manglerRet = person => person.querySelector("input:checked") === null;

function opdater() {
    for (const dag of dage) {
        const antalGaester = Number(dag.querySelector(".gaester-valg").value);

        dag.classList.toggle("spiser", spiser(dag));
        dag.querySelectorAll(".person[data-gaest]").forEach(p => p.hidden = Number(p.dataset.gaest) > antalGaester);

        // En ret er kun påkrævet for de personer, der vises
        const synlige = synligePersoner(dag);
        for (const person of dag.querySelectorAll(".person")) {
            const vises = synlige.includes(person);
            person.querySelectorAll("input").forEach(r => r.required = vises);
            person.classList.toggle("mangler", vises && manglerRet(person));
        }
    }

    const mangler = dage.flatMap(synligePersoner).filter(manglerRet).length;
    const aendret = tilstand() !== gemt;

    gemBar.classList.toggle("aendret", aendret || mangler > 0);
    status.textContent =
        mangler === 1 ? "Vælg en ret for 1 person, før du kan gemme" :
        mangler > 1 ? `Vælg en ret for ${mangler} personer, før du kan gemme` :
        aendret ? "Du har ændringer, der ikke er gemt" :
        "Alle valg er gemt";
}

formular.addEventListener("change", opdater);

formular.addEventListener("submit", e => {
    const foerste = dage.flatMap(synligePersoner).find(manglerRet);
    if (foerste) {
        e.preventDefault();
        foerste.scrollIntoView({ behavior: "smooth", block: "center" });
        foerste.querySelector("input").focus({ preventScroll: true });
        return;
    }
    sender = true;
});

// Advar hvis man forlader siden uden at gemme
window.addEventListener("beforeunload", e => {
    if (!sender && tilstand() !== gemt) e.preventDefault();
});

opdater();
