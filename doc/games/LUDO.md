# 🎲 Ludo Biofeedback

## 📋 Základní informace

- **Typ hry:** Relaxačně-soutěžní, pomalé tempo
- **Počet hráčů:** 2–4
- **Princip:** Klasické Ludo (Člověče, nezlob se), ale rychlost nebo přesnost hodu kostkou ovlivňuje úroveň relaxace hráče

## 🎯 Herní logika

- Hráč hází **"biofeedback kostkou"** – čím klidnější (nižší vodivost), tím vyšší číslo padne
- Pokud je hráč ve stresu, hází nižší čísla nebo kostka padne mimo rozsah
- **Cíl:** Dostat všechny figurky do domečku (klasické pravidla Luda)

## 📊 Biofeedback prvek

- **Relaxace** (GSR nízká) = lepší tah
- Měření probíhá při každém tahu
- API ukládá průběh celé hry v reálném čase

## 🗄️ Ukládaná data

```json
{
  "SessionId": "uuid",
  "UserId": "uuid", 
  "CurrentRelaxationValue": "float",
  "RoundTime": "datetime"
}
```

## 🎮 Herní mechanismus

1. **Příprava tahu:** Hráč se připraví na hod kostkou
2. **GSR měření:** Systém sleduje aktuální úroveň relaxace
3. **Výpočet hodu:** 
   - GSR nízká (relaxace) → vyšší pravděpodobnost 4-6
   - GSR vysoká (stres) → vyšší pravděpodobnost 1-3
4. **Provedení tahu:** Pohyb figurky podle výsledku
5. **Záznam dat:** Uložení GSR hodnoty a výsledku do databáze

## Výherní strategie

- **Dlouhodobá relaxace** během celé hry
- **Kontrola** před každým hodem
- **Klid** při rozhodování o tahu figurky