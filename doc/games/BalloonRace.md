# Balloon Race (Balónový závod)

Tato hra představuje implementaci kompetitivního biofeedbacku zaměřeného na techniky hluboké relaxace a snižování elektrodermální aktivity (EDA/GSR). Na rozdíl od běžných závodních her, kde je cílem rychlá reakce, v Balloon Race vítězí hráč, který dokáže nejefektivněji kontrolovat svou fyziologickou vzrušivost.

## Princip hry a mechanika pohybu

Hra simuluje závod horkovzdušných balónů na fixní vzdálenost (standardně $1000$ jednotek). Pohyb hráče je definován dvěma vektory:

- **Vertikální pohyb (Výška):**  
	Nadmořská výška balónu je přímo mapována na aktuální hodnotu biosignálu (GSR). Vizuální reprezentace výšky v rozhraní odpovídá naměřenému napětí či odporu, kde nižší úroveň stresu (nižší hodnota GSR) vede k poklesu balónu do klidových hladin, zatímco zvýšená excitace způsobuje jeho stoupání.

- **Horizontální pohyb (Vzdálenost):**  
	Dopředný pohyb je konstantní (2 jednotky za herní tik), pokud je hra aktivní. Vítězem se stává hráč, který jako první dosáhne cílové čáry (Finish Line).

## Technická realizace

- **Architektura:** Hra je implementována na principu klient‑server s využitím reálného času.

- **Serverová logika (BalloonGameService):**
	- Spravuje stav hry pomocí vláknově bezpečné kolekce (`ConcurrentDictionary`).
	- Validuje vstupy, vypočítává progres hráčů a detekuje stav vítězství.
	- Data o biofeedbacku jsou v reálném čase ukládána do databáze prostřednictvím asynchronní fronty (`DbWriteQueue`) pro následnou analýzu.

- **Klientská část (BalloonGame.tsx):**
	- Zajišťuje vizualizaci pomocí React komponent.
	- Komunikace se serverem probíhá přes SignalR, který umožňuje push‑notifikace o změně stavu všem připojeným klientům.

- **Zpracování signálu:**
	- Klient v intervalu 500 ms odesílá data ze senzoru (případně simulovaná data v demo režimu) na server.
	- Pro plynulost pohybu je na front‑endu využita CSS tranzice (`transition: all 0.5s ease-out`), která vyhlazuje skoky v naměřených hodnotách.

## Uživatelské rozhraní

- **Herní plocha (BalloonPlayground):**  
	Využívá dynamické mapování souřadnic, kde je výška balónu v pixelech vypočítána vztahem:

	$$
	y_{pos} = \\frac{GSR_{val}}{1000} \\cdot 400\\,\\mathrm{px}
	$$

	Tento výpočet zajišťuje, že se balón pohybuje v definovaném vertikálním poli bez ohledu na rozlišení obrazovky.

- **Informace pro hráče:**  
	Hráči mají k dispozici informační karty (`PlayerInfoCard`) s přesnými číselnými údaji o aktuální výšce a celkovém postupu v procentech.

---

Potřebuješ, abych přidal další podsekce (např. parametrizaci hry, telemetrii nebo datové schéma pro ukládání)?

