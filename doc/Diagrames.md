# Documentació Tècnica: Five Night's at Pedralbes

## 1. Diagrama Entitat-Relació (E/R)
Aquest diagrama mostra l'estructura de dades a la base de dades MongoDB i com es relacionen les col·leccions:

```mermaid
erDiagram
    USER ||--o{ GAME : hosts
    USER ||--o{ GAME : plays
    USER ||--o{ RESULT : has
    USER {
        string username
        string password
        object stats
        date createdAt
    }
    GAME {
        string roomCode
        objectId hostId
        objectIdArray players
        string status
        int maxPlayers
        date createdAt
    }
    RESULT {
        objectId userId
        boolean survived
        int livesRemaining
        string timeSurvived
        date createdAt
    }
```

## 2. Diagrama d'Arquitectura
Descripció de la infraestructura i el flux de comunicació entre els diversos components del sistema:

```mermaid
graph TD
    subgraph Client_Side
        U[Unity Client]
        C[C# / UI Toolkit]
    end
    subgraph Server_Side
        NX[Nginx Proxy]
        D[Docker Container]
        S[Node.js Server]
        SIO[Socket.io - Realtime]
        DB[(MongoDB - Persistence)]
    end
    U <--> NX
    NX <--> D
    D <--> S
    S <--> SIO
    S <--> DB
```
