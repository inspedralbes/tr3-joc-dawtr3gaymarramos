# Prompts Log: Spec-Driven Development (SDD)

## Informació de Sessió
**Funcionalitat:** Sistema de "Ready" a la Lobby.
**Metodologia:** SDD assistit per IA.

---

### [ITERACIÓ 1] Definició de l'Especificació (OpenSpec)
**Data:** (Inici)
**Objectiu:** Definir el context, comportament i pla de desenvolupament abans de començar a programar.

**Prompt usat (Usuari):**
> "tengo otro apartado, que te parece que podemos hacer Us d’IA amb desenvolupament guiat per especificació (OpenSpec) [...] la opcion 1 me gusta Opción 1: Sistema de "Ready" (Preparado) en la Lobby"

**Resultat de la IA:**
S'han generat correctament els fitxers a la carpeta `specs/`:
1. `foundations.md`
2. `spec.md`
3. `plan.md`
També s'ha generat aquest fitxer de registre `prompts-log.md` a la carpeta `docs/`.

---

### [ITERACIÓ 2] Primera passada de codi (Implementació)
**Data:** (Avui)
**Objectiu:** Executar el codi previst al pla d'implementació.

**Prompt usat (Usuari):**
> "al meu profe li dona igual aixi que anem per lliure" *(Interpretat com a "aplica l'especificació i programa-ho").*

**Resultat de la IA:**
S'han modificat els següents fitxers segons el `plan.md`:
1. `server/src/app.js`: S'ha afegit l'escoltador `socket.on('playerReady')` que emet `opponentReady`.
2. `LobbyScreen.uxml`: S'ha afegit el botó `BtnReady` (amagat per defecte) i l'etiqueta `LblStatus` al costat del Player 2.
3. `LobbyScreen.cs`: S'ha afegit la lògica per amagar/mostrar els botons segons el rol (`esHost`), enviar la senyal de *Ready* al servidor i habilitar el botó de començar partida quan es rep la confirmació.

**Estat:** Pendent de proves per part de l'usuari. Si es troba algun error, es farà una Iteració 3 de correcció de bugs.

---

### [ITERACIÓ 3] Correccions i Poliment
**Data:** (Avui)
**Objectiu:** Arreglar que el jugador no es pugui moure quan mor i aclarir el flux d'inici de partida.

**Prompt usat (Usuari):**
> "em surt pero no inicia la partida en el jugador2 tinc que donar a "estic llest" solament aixo, despres pots fer que quan te quedes abatut tant en individual com multijugador no et puguis moure"

**Resultat de la IA i anàlisi de desviació:**
* L'usuari esperava que en prémer "Estic Llest", la partida s'iniciés automàticament. L'agent d'IA ha aclarit que, segons el `spec.md` definit a la Iteració 1, premer "Estic Llest" només habilita el botó "Començar Partida" del Host. És el Host qui té l'autoritat per donar l'inici definitiu.
* Per solucionar el moviment en caure abatut, s'ha detectat que la variable `scriptMovimiento` podia quedar buida a l'Inspector de Unity. La IA ha modificat `PlayerHealth.cs` per forçar la desactivació del component `PlayerMovement` de forma automàtica (`GetComponent<PlayerMovement>().enabled = false;`).

**Estat:** Codi final tancat i llest per a demostració.
