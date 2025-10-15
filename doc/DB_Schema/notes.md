# Database Design - Biofeedback Gaming Platform

## Přehled
Databáze slouží k ukládání uživatelských dat, herních místností, biofeedback senzorových dat a statistik pro platformu biofeedback her.

## Tables

### User
**Účel:** Ukládání uživatelských účtů, autentifikace a základních profilových informací.

- `id` - Unikátní identifikátor uživatele (UUID pro distribuované systémy)
- `username` - Uživatelské jméno (unikátní, max 100 znaků)
- `email` - Emailová adresa pro komunikaci a resetování hesla
- `password_hash` - Zabezpečený hash hesla (bcrypt)
- `created_at` - Datum registrace účtu
- `last_login` - Poslední přihlášení pro sledování aktivity

**Poznámky:**
- Hesla se nikdy neukládají v plaintextu
- `last_login` pomáhá identifikovat neaktivní účty

### GameRoom
**Účel:** Správa herních místností pro multiplayer hry s biofeedback.

- `id` - Unikátní identifikátor místnosti
- `name` - Název místnosti (zobrazovaný hráčům)
- `game_type` - Typ hry: `"relax_game"`, `"focus_game"`, `"stress_test"`, `"meditation"`
- `max_players` - Maximální počet hráčů (obvykle 2-8)
- `status` - Stav místnosti:
  - `"waiting"` - čeká na hráče
  - `"active"` - hra probíhá  
  - `"finished"` - hra skončena
- `password_hash` - Hash hesla pro soukromé místnosti (NULL = veřejná)
- `created_at` - Čas vytvoření místnosti
- `created_by` - ID zakladatele místnosti

**Poznámky:**
- Soukromé místnosti vyžadují heslo pro vstup
- `game_type` určuje herní logiku a UI
- Status pomáhá filtrovat dostupné místnosti

### Session
**Účel:** Sledování herních sezení jednotlivých hráčů v konkrétních místnostech.

- `id` - Unikátní identifikátor sezení
- `user_id` - Odkaz na hráče
- `game_room_id` - Odkaz na herní místnost
- `start_time` - Začátek sezení
- `end_time` - Konec sezení (NULL dokud hra pokračuje)
- `is_active` - Označuje aktivní sezení

**Poznámky:**
- Jeden user může mít více sessions (historických)
- `is_active` = false po ukončení hry nebo odpojení
- Umožňuje sledovat délku hraní a reconnect logiku

### BioFeedback
**Účel:** Vysokofreuenční záznam biofeedback dat (GSR senzor) během herních sezení.

- `id` - Autoincrement ID (int pro rychlost a úsporu místa)
- `session_id` - Vazba na konkrétní herní sezení
- `gsr_value` - Hodnota kožní reakce
- `timestamp` - Přesný čas měření

**Poznámky:**
- **High-frequency data** - může generovat 10-100+ záznamů za sekundu
- Int ID místo UUID kvůli performance a velikosti
- Vyžaduje indexování na `session_id` a `timestamp`

### Statistic
**Účel:** Agregované statistiky uživatelů pro jednotlivé typy her.

- `id` - Unikátní identifikátor statistiky
- `user_id` - Odkaz na uživatele
- `game_type` - Typ hry pro kterou je statistika
- `average_gsr` - Průměrná GSR hodnota napříč všemi sezeními
- `best_score` - Nejlepší dosažené skóre v daném typu hry
- `total_sessions` - Celkový počet odehraných sezení
- `last_played` - Datum posledního hraní tohoto typu hry

**Poznámky:**
- Jeden user má jeden záznam na `game_type`
- Statistiky se přepočítávají po každém dokončeném sezení
- `best_score` závisí na typu hry (vyšší/nižší = lepší)

### RefreshToken
**Účel:** Správa refresh tokenů pro JWT autentifikaci a bezpečné obnovování přístupových tokenů.

- `id` - Unikátní identifikátor refresh tokenu (UUID)
- `user_id` - Odkaz na uživatele, kterému token patří
- `token` - Samotný refresh token (hash nebo encrypted string)
- `created` - Čas vytvoření tokenu
- `expires` - Čas vypršení platnosti tokenu
- `revoked` - Čas zrušení tokenu (NULL = aktivní token)
- `revoked_by_ip` - IP adresa, ze které byl token zrušen (pro audit)
- `replaced_by_token` - Nový token, kterým byl tento nahrazen (pro token rotation)

**Poznámky:**
- **Token rotation** - při každém refreshi se vytvoří nový token a starý se označí jako nahrazený
- `revoked` není NULL při manuálním odhlášení, bezpečnostním incidentu nebo vypršení
- `revoked_by_ip` pomáhá při forensním auditu a detekci podezřelé aktivity
- `replaced_by_token` tvoří řetězec tokenů pro sledování token family
- Jeden uživatel může mít více aktivních refresh tokenů (různá zařízení)


## Potenciální rozšíření
- **Achievement** - herní úspěchy/odznaky
- **Tournament** - turnaje a soutěže  
- **GameSettings** - uživatelská nastavení her
- **Notification** - in-app notifikace
- **Chat** - in-app chat
