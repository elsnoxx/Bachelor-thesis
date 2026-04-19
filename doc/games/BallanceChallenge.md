Tato hra je postavena na principu biofeedbacku. Funguje jako "digitální váha" stresu, kde se snažíte udržet kuličku v bezpečné zóně pomocí vlastního zklidnění.

Zde je podrobný rozbor toho, jak se počítají hodnoty a jaký je herní mechanismus:

1. Fáze Kalibrace (Prvních 10 sekund)
Hra nezačne hned. Čeká se na připojení obou hráčů. Jakmile jsou v místnosti oba, spustí se StartTime.

Sběr dat: Během prvních 10 sekund se plní historie (_leftHistory, _rightHistory).

Nastavení Baseline: V momentě konce kalibrace se nastaví tzv. Baseline (klidová úroveň). V tvém aktuálním kódu se bere minimum naměřené během kalibrace.

Příklad: Pokud jsi měl při kalibraci hodnoty 5500, 5200 a 5800, baseline bude 5200. Vše nad tuto hodnotu už se počítá jako "stoupající stres".

2. Výpočet pozice kuličky (GetBallPosition)
Pozice kuličky (0 až 100) je součtem dvou složek: SCL (dlouhodobý stav) a SCR (krátkodobý pík).

A) Složka SCL (Tónická úroveň – "Jak moc jsi napjatý")
Vypočítá se jako rozdíl aktuálního průměru a baseline:
leftDiff = PrůměrnáHodnota - Baseline
sclBase = leftDiff / sensitivity (kde citlivost je nastavena na 40).

Příklad: Máš průměr 5600 a baseline 5200. Rozdíl je 400. 400 / 40 = 10. Kulička se posune o 10 % doprava.

B) Složka SCR (Fázická úroveň – "Leknutí / Škubnutí")
V metodě AddValue sleduješ náhlé skoky.

Pokud nová hodnota skočí o více než 100 jednotek oproti minulé, přičte se k penalizaci _scrPenalty okamžitě +15.

Tato penalizace ale není trvalá. V každém kroku se násobí 0.97 (_scrPenalty *= 0.97), takže pomalu "vyprchává", jak se uklidňuješ.

Celková pozice = Vyšší hodnota SCL (jednoho z hráčů) + Aktuální SCR penalizace.

3. Dynamická cílová zóna (UpdateTargetZone)
Zelená zóna, ve které se musíte udržet, není statická. Mění se podle toho, jak hrajete:

Odpočinek: Pokud jsou oba hráči blízko své baseline (rozdíl do 150), zóna se rozšiřuje o 0.05 každých pár milisekund (až do 80 % šířky obrazovky). Hra se stává lehčí.

Stres: Pokud aspoň jeden hráč začne "stresovat" (rozdíl nad 150), zóna se začne zmenšovat o 0.1 (minimálně na 40 %). Hra se stává těžší a kulička snadněji vypadne.

4. Podmínky konce hry (IsGameOver)
Hra končí ve dvou případech:

Vypršení času: Pokud uplyne DurationSeconds (120 s). Pokud je v ten moment kulička v zóně, je to Vítězství.

Přílišný stres: Pokud kulička překročí hranici TargetMax (pravý okraj zelené zóny).

Poznámka: V tvém aktuálním kódu jsi řádek pro prohru stresu zakomentoval (//if (gracePeriodOver && GetBallPosition() >= TargetMax) return true;), takže momentálně hra končí až časem. Pokud to odkomentuješ, hra skončí okamžitě, jakmile kulička vyletí ze zóny.

5. Jak funguje ukládání
Služba BallanceGameService hlídá stav:

Jakmile IsGameOver vrátí true, zavolá se SaveGameResult.

Vítězství: Je definováno jako isWin = IsGameOver && zbývající čas <= 0 && kulička je v zóně.

Výsledek se zapíše do databáze a místnost se uzavře (NotifyRoomStatus(roomId, RoomStatus.Finish)).