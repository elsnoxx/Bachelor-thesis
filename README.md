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

## Roadmap & TODO
Seznam nedokončených funkcí a plánovaných vylepšení je dostupný v [TODO dokumentaci](doc/Todos.md).

## Licence

Tento projekt je licencován pod licencí [MIT](./LICENSE).

Podle všeho celé ovládání je založeno na SCL? Nikoliv na SCR? Ale nikde jsem přesně to ovládání nezachytil, chybí mi tam popis proč a jak, ale SCL se mění pomalu a změna v SCL je zdlouhavá, navíc SCL je ovlivněno teplotou okolí a fyziologií uživatele, jak zajístíte, že jedinec, který má po nasazení hodnotu 45 000 je porovnatelný s jedincem co má po nasazení hodnotu 15 000? Přece jenom změna ze 45000 na 15000 je otázkou spíše desítek minut a nikoliv sekund a pokud je to muž, tak se pravděpodobně nikdy na hodnotu 15 000 nedostane.

Jako on tan biofeedback bude asi fungovat i na SCL, ale pokud muzete hodte tam treba i nejakou metriku, takova smerodatna odchylka bude fungovat asi docela dobre
