# Memòria de l'Entrega: Desenvolupament Guiat per Especificació (OpenSpec)

## 1. Explicació de la funcionalitat
La funcionalitat principal implementada mitjançant la metodologia SDD (Spec-Driven Development) ha estat el **Sistema de "Ready" (Preparat) a la Lobby Multijugador**.

Aquest sistema resol el problema de coordinació entre jugadors abans de començar una partida. En un joc multijugador, és crucial que el Host (el creador de la sala) sàpiga quan el Client (el segon jugador) està realment a punt per començar.
- **Client:** Disposa d'un botó "Estic Llest" que notifica la seva disponibilitat.
- **Servidor:** Actua com a intermediari, rebotant la senyal de preparació cap al Host.
- **Host:** El botó "Començar Partida" es manté bloquejat fins que rep la notificació que el seu oponent està a punt.

A més, s'ha implementat una millora de gameplay on els jugadors perden la capacitat de moviment en quedar abatuts (mort), assegurant la coherència entre l'estat de salut i el control del personatge.

## 2. Procés seguit amb la IA
El procés s'ha dividit en tres etapes clarament diferenciades seguint el cicle de vida d'OpenSpec:

1.  **Fase de Disseny (OpenSpec):** Abans de tocar cap línia de codi, es va demanar a la IA definir l'especificació a la carpeta `specs/`. Es van generar els fitxers `foundations.md` (context), `spec.md` (comportament esperat) i `plan.md` (passos tècnics).
2.  **Fase d'Implementació:** Un cop aprovat el pla, la IA va procedir a modificar els fitxers del servidor (Node.js) i del client (Unity/C# i UXML) de forma coordinada.
3.  **Fase de Refinament:** Es van realitzar ajustos basats en el feedback de les proves, com la correcció del moviment post-mort i l'aclariment de qui té l'autoritat per iniciar la partida.

## 3. Principals problemes trobats
- **Sincronització de WebSockets:** Un dels reptes va ser assegurar que la senyal de `opponentReady` arribés correctament al Host en el context d'una sala específica, evitant que altres sales rebin senyals que no els pertanyen.
- **Referències de Components a Unity:** En la lògica de salut, es va trobar un problema on el component de moviment no es desactivava correctament perquè la referència a l'Inspector s'havia perdut. Es va solucionar forçant la cerca del component en temps d'execució via codi.
- **Flux d'UI:** Inicialment, el botó de "Començar Partida" podia induir a confusió si no hi havia un feedback visual clar sobre si el segon jugador estava llest o no.

## 4. Decisions preses (canvis en prompts o spec)
- **Autoritat del Host:** Es va decidir mantenir que només el Host pugui prémer "Començar Partida" fins i tot després que el Client estigui "Ready". Això evita que la partida comenci de forma sobtada sense que el Host (qui potser està esperant altres configuracions) estigui d'acord.
- **Desactivació de Moviment vs. Destrucció:** En lloc de destruir l'objecte jugador en morir, es va decidir simplement desactivar el component `PlayerMovement`. Això permet mantenir el personatge visible en l'estat d'abatut per a possibles futures funcionalitats de "reanimació" o simplement per visualitzar qui ha mort.
- **Prompting Directe:** Es va canviar l'estil de prompts de "fes-ho tot" a "segueix el pla definit a `specs/`", la qual cosa va reduir les al·lucinacions de la IA i va assegurar que el codi seguís l'arquitectura desitjada.

## 5. Valoració crítica real
L'ús de la metodologia OpenSpec amb la IA ha demostrat ser molt eficaç per a projectes multijugador complexos. 

**Punts positius:**
- **Claredat:** Tenir un document de referència (`spec.md`) evita que la IA perdi el context de què estem intentant aconseguir a mesura que el projecte creix.
- **Menys Errors:** El pla d'implementació previ ajuda a detectar conflictes de dependències abans d'escriure el codi.

**Punts de millora:**
- **Rigidesa:** De vegades, si el pla inicial té una petita errada conceptual, la IA la segueix al peu de la lletra fins que l'usuari la corregeix manualment en una iteració posterior.
- **Dependència del Context:** Encara cal una supervisió humana constant per verificar que les referències a Unity (UXML, botons) coincideixen exactament amb el que hi ha a l'Editor.

En conclusió, el desenvolupament guiat per especificació és una eina poderosa que eleva la qualitat del programari i redueix el temps de *debugging*, sempre que el programador mantingui el control sobre el disseny d'alt nivell.
