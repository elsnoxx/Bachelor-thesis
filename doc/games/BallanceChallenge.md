# Balance Game

## Základní informace

- **Typ hry:** Kooperativní multiplayer hra
- **Počet hráčů:** 2-4
- **Princip:** Na obrazovce je koule na plošině, kterou všichni hráči společně ovládají pomocí svých GSR hodnot. Cílem je udržet kouli ve středu po stanovený čas (5 minut).

## Herní logika

- **Kooperativní ovládání** - koule reaguje na průměrnou GSR hodnotu všech hráčů
- Všichni hráči musí **koordinovaně relaxovat** aby udrželi kouli stabilní
- Pokud jeden hráč dostane stres, ovlivní to celý tým
- **Časový limit:** Tým musí udržet kouli ve střední zóně po dobu 5 minut

## Biofeedback prvek

- **Koule jako vizuální metafora** týmové rovnováhy a synchronizace
- **Real-time feedback** - okamžitá reakce na změny průměrné GSR hodnoty týmu
- API ukládá průběh hry v krátkých časových úsecích (každých 200 ms)
- **Týmové skóre stability** - hodnocení kvality spolupráce

## Ukládaná data

```json
{
  "SessionId": "uuid",
  "TeamAverageGSR": "float",
  "BalanceDeviation": "float (-1.0 to 1.0)", 
  "IndividualGSRValues": [
    {"UserId": "uuid", "GSRValue": "float"},
    {"UserId": "uuid", "GSRValue": "float"}
  ],
  "TimeStamp": "datetime",
  "TeamStabilityScore": "int (0-100)"
}