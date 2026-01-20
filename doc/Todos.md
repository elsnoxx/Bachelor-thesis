## Todos

### DB
- Biofeedback values 
    - **Retention policy** - data starší než 6-12 měsíců lze archivovat/mazat
    - Indexování podle `session_id` a `timestamp`
- **Cleanup job** 
    - pravidelně mazat expirované a zrušené tokeny starší než X dní, pomoci backgroud workera

### API server
- **GameRoom Controller**
    - Zobrazení všech dostupných sessions - upravit json 

- **Statistics Controller**
    - Zobrazení výsledků a statistik hráčů
    - Real-time zobrazení biofeedback dat během hry
    - Historický přehled GSR hodnot pro analýzu
    - Leaderboards a achievement systém

### GameHub (SignalR)
- **Session Management**
    - Vytvoření session pouze pro uživatele, který spustil místnost
    - Automatické připojení hráčů do aktivní session
    - Odpojování neaktivních hráčů z session

- **Herní logika**
    - **Ludo** - tahová hra s GSR kontrolou rychlosti pohybu
    - **Energy Battle** - real-time souboj založený na GSR intenzitě
    - **Balance Game** - udržení GSR hodnot v cílovém rozmezí