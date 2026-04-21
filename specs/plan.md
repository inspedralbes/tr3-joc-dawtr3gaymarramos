# Implementation Plan

## Fase 1: Backend (Node.js)
1. Modificar `server/src/app.js`.
2. Dins de l'esdeveniment `connection`, afegir un nou `socket.on('playerReady', (roomCode) => { ... })`.
3. Dins d'aquest esdeveniment, rebotar la senyal a la sala amb `socket.to(roomCode).emit('opponentReady')`.

## Fase 2: Interfície (UI Toolkit)
1. Modificar `Unity/JocAymarRamos/Assets/LobbyScreen.uxml`.
2. Afegir un nou botó `<ui:Button name="BtnReady" text="Estic Llest" class="menu-button" />` just a sota o al lloc del botó d'inici.
3. Configurar-lo visualment per mantenir la coherència estètica amb la resta de botons.

## Fase 3: Client (Unity C#)
1. Modificar `LobbyScreen.cs`.
2. Referenciar el nou element `btnReady`.
3. Ocultar o mostrar `btnReady` depenent de si és Host o Client (`PlayerPrefs.GetInt("esHost")`).
4. Si ets Client, afegir el `clicked` event a `btnReady` per emetre `playerReady` i deshabilitar-se a si mateix.
5. Si ets Host, assegurar que `btnStart` comença deshabilitat (`SetEnabled(false)`).
6. Afegir la subscripció `socket.OnUnityThread("opponentReady", ...)` perquè el Host habiliti el botó `btnStart` quan la rebi.
