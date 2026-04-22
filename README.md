# Biofeedback Multiplayer Games (Bakalářská práce)

Fyziologické veličiny poskytují objektivní náhled na psychické procesy jednotlivce. Jejich měřením lze analyzovat například míru stresu, kterému je člověk vystaven, nebo se pokusit rozpoznat emoce, které právě prožívá. 

Jedním z přístupů, jak s těmito složitými procesy vědomě pracovat, je metoda **biofeedbacku**. Ta umožňuje uživateli získávat v reálném čase informace o svých fyziologických funkcích a pomocí vědomého úsilí je ovlivňovat.

Cílem této bakalářské práce je navrhnout a naprogramovat **hry pro více hráčů**, které budou ovládány pomocí **vodivosti pokožky** a umožní uživatelům trénink relaxace či aktivace **zábavnou formou** prostřednictvím biofeedbacku.

---

## Hlavní cíle práce

1. Nastudovat problematiku vodivosti pokožky.
2. Nastudovat problematiku měřícího zařízení a jeho komunikaci prostřednictvím Bluetooth Low Energy technologie.
3. Navrhnout a naprogramovat alespoň **tři hry pro více hráčů**, jejichž ovládání bude založeno na změnách vodivosti pokožky.
4. Ověřit funkčnost navrženého řešení.

---

## Dokumentace 
- [Kompletní dokumentace](/doc/TechnicalSpec/README.md) - technická specifikace, API dokumentace
- [Databázové schéma](/doc/DB_Schema/notes.md) - návrh databáze
- [Herní mechaniky](/doc/games/) - popis jednotlivých her
---


## Spuštění aplikace

Tuto aplikaci lze provozovat buď jako kompletní celek pomocí kontejnerizace, nebo spouštět jednotlivé části (backend a frontend) samostatně pro účely vývoje a ladění.

### 1. Kompletní spuštění (Docker Compose)

Nejjednodušším způsobem, jak zprovoznit celou infrastrukturu včetně databáze, je použití nástroje **Docker Compose**. Tento přístup zaručuje, že všechna prostředí budou správně nakonfigurována.

1. Otevřete terminál v kořenové složce projektu `/Bachelor-thesis`.
2. Spusťte příkaz pro sestavení a spuštění kontejnerů:

```bash
docker compose up --build
```

Zakladní uživatel: admin@example.com, admin

Upozornění: První sestavení může trvat delší dobu, protože je nutné stáhnout základní obrazy (Docker Images) a následně provést kompilaci aplikací.

V případě potřeby lze upravit parametry (např. porty nebo databázová hesla) přímo v souboru `docker-compose.yml`, kde jsou přednastaveny hodnoty použité během testování.

### 2. Samostatné spuštění (Vývojářský režim)

Pokud potřebujete spustit služby samostatně bez využití Dockeru, postupujte podle následujících kroků:

#### A. Backend (API a WebSocket server)

Serverová část je postavena na platformě .NET. Pro její úspěšný běh je nezbytné mít přístup k běžící instanci databáze.

1. Přejděte do složky se serverovou částí aplikace.
2. Proveďte sestavení a spuštění příkazem:

```bash
dotnet run
```

Server se spustí a začne naslouchat na definovaném portu. Pokud aplikace nenaváže spojení s databází, proces se ukončí s chybou.

#### B. Frontend (UI React aplikace)

Webové rozhraní využívá framework React a pro správu balíčků nástroj npm.

1. Přejděte do složky s frontendovou aplikací.
2. Spusťte instalovaní balíčků npm:
```bash
npm install
```
2. Spusťte vývojový server příkazem:
```bash
npm run dev
```

Aplikace bude dostupná na adrese `localhost` (zpravidla port 5173). Konfiguraci adresy backendového serveru pro směrování API požadavků a WebSocket komunikace lze upravit v souboru `.env`.

## Licence

Tento projekt je licencován pod licencí [MIT](./LICENSE).

Podle všeho celé ovládání je založeno na SCL? Nikoliv na SCR? Ale nikde jsem přesně to ovládání nezachytil, chybí mi tam popis proč a jak, ale SCL se mění pomalu a změna v SCL je zdlouhavá, navíc SCL je ovlivněno teplotou okolí a fyziologií uživatele, jak zajístíte, že jedinec, který má po nasazení hodnotu 45 000 je porovnatelný s jedincem co má po nasazení hodnotu 15 000? Přece jenom změna ze 45000 na 15000 je otázkou spíše desítek minut a nikoliv sekund a pokud je to muž, tak se pravděpodobně nikdy na hodnotu 15 000 nedostane.

Jako on tan biofeedback bude asi fungovat i na SCL, ale pokud muzete hodte tam treba i nejakou metriku, takova smerodatna odchylka bude fungovat asi docela dobre
