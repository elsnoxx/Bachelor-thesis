## Todos

- MQTT / BackgroundWorker
    - Přebírá kontinuální proud dat z BLE → ukládá do DB.

### DB
- Omezení velikosti znaku u username na 100 
- Biofeedback values 
    - **Retention policy** - data starší než 6-12 měsíců lze archivovat/mazat
    - Indexování podle `session_id` a `timestamp`
- pridat indexy v entity frameworku
    - idx_session_user_active ON Session(user_id, is_active) 
    - idx_biofeedback_session_time ON BioFeedback(session_id, timestamp)
    - idx_gameroom_status_type ON GameRoom(status, game_type)
    - idx_statistic_user_game ON Statistic(user_id, game_type)
- **Cleanup job** 
    - pravidelně mazat expirované a zrušené tokeny starší než X dní

### API server
- **GameRoom Controller**
    - Přidání uživatele do session
    - Opuštění session
    - Zobrazení všech dostupných sessions
    - Vytváření nových herních místností
    - Správa heslem chráněných místností

- **Statistics Controller**
    - Zobrazení výsledků a statistik hráčů
    - Real-time zobrazení biofeedback dat během hry
    - Historický přehled GSR hodnot pro analýzu
    - Leaderboards a achievement systém

- **General**
    - Automatické spuštění migrací při startu aplikace
    - Seed data pro prázdnou databázi
    - Health check endpoints pro monitoring

### GameHub (SignalR)
- **Session Management**
    - Vytvoření session pouze pro uživatele, který spustil místnost
    - Automatické připojení hráčů do aktivní session
    - Odpojování neaktivních hráčů z session

- **Herní logika**
    - **Ludo** - tahová hra s GSR kontrolou rychlosti pohybu
    - **Energy Battle** - real-time souboj založený na GSR intenzitě
    - **Balance Game** - udržení GSR hodnot v cílovém rozmezí