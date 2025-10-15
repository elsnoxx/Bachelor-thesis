## Použité technologie

### Frontend
- **React 18** + **TypeScript** - moderní webová aplikace s type safety
- **Vite** - rychlý build tool a dev server
- **Web Bluetooth API** - přímá komunikace s BLE zařízeními v prohlížeči

### Backend
- **ASP.NET Core 8** - RESTful API server
- **SignalR** - real-time WebSocket komunikace pro multiplayer funkcionalitu
- **Entity Framework Core** - ORM pro databázové operace


### Databáze
- **PostgreSQL** - relační databáze pro persistentní data

```bash
# PostgreSQL databáze
docker run -d --name postgres \
  -e POSTGRES_DB=biofeedback \
  -e POSTGRES_USER=admin \
  -e POSTGRES_PASSWORD=password \
  -p 5432:5432 postgres:15
```

### Hardware & Komunikace
- **GSR senzor** (Galvanic Skin Response) - měření vodivosti pokožky
- **Bluetooth Low Energy (BLE)** - bezdrátová komunikace se senzory

### DevOps & Nástroje
- **Docker** - kontejnerizace aplikace
- **Git** - verzování kódu

### Deployment & Spuštění

**Docker Compose** - orchestrace všech služeb (backend, database, frontend)