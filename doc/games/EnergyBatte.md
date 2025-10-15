# Energy Battle

## Základní informace

- **Typ hry:** Soutěžní real-time souboj
- **Počet hráčů:** 2
- **Princip:** Dva hráči soupeří proti sobě v přímém souboji. Každý se snaží udržet ideální hladinu aktivace GSR, aby "nabíjel" svou energii rychleji než soupeř a porazil ho.

## Herní logika

- **Přímý souboj** - dva hráči soupeří simultánně v real-time
- Hráč dostává hodnoty GSR v reálném čase
- **Ideální zóna aktivace** (např. 0.4–0.6) nabíjí "energetický ukazatel"
- Když vystoupí mimo rozsah (příliš nervózní nebo příliš klidný), nabíjení se zpomalí
- **Vyhrává ten, kdo dosáhne plného ukazatele jako první**
- Hra může trvat 2-10 minut podle úrovně hráčů

## Biofeedback prvek

- **Cílem je kontrola úrovně stresu** – ani příliš vysoká, ani příliš nízká
- **Sweet spot aktivace** - najít optimální úroveň GSR pro rychlé nabíjení
- **Real-time soutěž** - okamžitá reakce na změny GSR obou hráčů
- API ukládá pokrok každého hráče každých 500ms
- **Vizuální feedback** - energetické ukazatele obou hráčů vedle sebe

## Ukládaná data

```json
{
  "SessionId": "uuid",
  "Player1": {
    "UserId": "uuid",
    "EnergyLevel": "float (0.0-1.0)",
    "CurrentGSR": "float",
    "InTargetZone": "boolean",
    "TargetZoneHitCount": "int"
  },
  "Player2": {
    "UserId": "uuid", 
    "EnergyLevel": "float (0.0-1.0)",
    "CurrentGSR": "float",
    "InTargetZone": "boolean",
    "TargetZoneHitCount": "int"
  },
  "TimeElapsed": "int (seconds)",
  "GameStatus": "string (active/finished)"
}