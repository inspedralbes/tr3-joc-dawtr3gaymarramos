# Specification: Comportament Esperat

## Lògica de la Interfície (UI)
1. **Per al Host (Creador de la sala):**
   * El botó "Començar Partida" (`BtnStartGame`) ha d'estar deshabilitat (`SetEnabled(false)`) per defecte tan bon punt s'uneix a la sala, fins i tot si entra un company.
   * Quan el Client confirmi que està llest, el botó "Començar Partida" s'ha d'habilitar automàticament perquè el Host pugui arrencar el joc.

2. **Per al Client (Unint-se a la sala):**
   * El botó "Començar Partida" es manté ocult (`DisplayStyle.None`) tal com està ara.
   * Ha de veure un nou botó anomenat "Estic Llest" (`BtnReady`).
   * En prémer "Estic Llest", aquest botó s'ha de deshabilitar per evitar *spam* i s'ha d'enviar la senyal al servidor.

## Lògica de Xarxa (WebSockets)
1. Quan el Client prem "Estic Llest", l'script `LobbyScreen.cs` emet un esdeveniment `playerReady` al servidor passant l'ID de la sala (`roomCode`).
2. El servidor Node.js (`app.js`) rep l'esdeveniment i fa un broadcast (`socket.to(room).emit('opponentReady')`) a la resta de la sala.
3. El Host rep l'esdeveniment `opponentReady` a través de `SocketHandler.socket` i la seva interfície s'actualitza habilitant el botó d'inici.
