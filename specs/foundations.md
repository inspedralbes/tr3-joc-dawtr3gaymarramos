# Foundations: Sistema de "Ready" a la Lobby

## Context
Actualment, a la pantalla de Lobby (LobbyScreen), quan dos jugadors es connecten a una sala multijugador, el Host té el control absolut de l'inici de la partida. Si el Host prem "Començar Partida", el joc s'inicia immediatament per a tots dos usuaris, independentment de si el company està llest o no.

## Objectius
* Implementar un sistema de confirmació on l'usuari Client pugui indicar que està preparat ("Ready").
* Bloquejar l'inici de la partida per part del Host fins que el Client hagi confirmat el seu estat.
* Millorar la comunicació visual a la UI de la Lobby perquè tots dos jugadors sàpiguen l'estat de l'altre.

## Restriccions
* S'ha d'utilitzar UI Toolkit (UXML) per afegir el nou botó/text a la interfície.
* La comunicació de l'estat "Ready" s'ha de fer a través de WebSockets (Socket.io) utilitzant el backend en Node.js existent.
* No s'ha de recarregar l'escena innecessàriament, tot ha de ser dinàmic a la mateixa `LobbyScreen`.
