# Pip y el Rey Babosa - Juego de plataformas 2D (Unity)

Juego de plataformas 2D estilo Mario hecho en Unity 6 para la materia Programación de Video Juegos.

## Autor

- **Estudiante:** Edwin Jesús Flete Cruz
- **Matrícula:** 1-22-1805
- **Universidad:** UTESA, Sede Santiago
- **Asignatura:** Programación de Video Juegos (INF-910-001)
- **Profesor:** Iván Mendoza
- **Motor:** Unity 6 (6000.6.4f1), URP 2D, C#, Input System

## Enlaces

- **Jugar en el navegador (GitHub Pages):** https://wingsofrebel.github.io/pip-y-el-rey-babosa/
- **Repositorio:** https://github.com/WingsOfRebel/pip-y-el-rey-babosa

## Historia

El Rey Babosa se robó las gemas del valle y se encerró en su castillo de piedra y lava.
Pip, un pequeño explorador verde, tiene que cruzar el Valle Verde y el Desierto Dorado para
llegar al castillo y vencerlo.

## Controles

| Acción | Teclas |
|---|---|
| Moverse | A / D o flechas izquierda / derecha |
| Saltar (mantener para saltar más alto) | Espacio, W o flecha arriba |
| Pausa | Esc o P |
| Empezar desde el menú | Enter o botón JUGAR |

## Reglas

- Empiezas con 3 vidas. Cada 50 monedas ganas una vida extra.
- Si tocas un enemigo, unas púas, la lava o caes a un hoyo, pierdes una vida y repites el nivel.
- Si te quedas sin vidas aparece **"¡EL PERSONAJE MURIÓ!"** y vuelves al nivel 1.
- Cada nivel tiene un tiempo límite. En el nivel 2 o 3, si se acaba el tiempo, vuelves al nivel 1.
- Al llegar a la bandera pasas al siguiente nivel. En el nivel 3 la bandera aparece al vencer al jefe.
- Puntos: moneda 10, gema 100, enemigos 100 a 200, jefe 1000 y bono por el tiempo que sobre.

## Niveles

1. **Valle Verde:** hoyos, púas, babosas, caracoles y moscas.
2. **Desierto Dorado:** ranas saltarinas, babosas de púas, sierras, un muro con resorte.
3. **Castillo del Rey Babosa:** pozos de lava y la pelea contra el jefe (música distinta).

## Enemigos y obstáculos

| Tipo | Qué hace | ¿Se puede pisar? |
|---|---|---|
| Babosa | Camina y se da vuelta en los bordes | Sí |
| Caracol | Camina lento | Sí |
| Mosca | Vuela de lado a lado ondulando | Sí |
| Rana | Salta hacia el jugador | Sí |
| Babosa de púas | Camina; tiene púas arriba | No |
| Sierra | Va y viene girando | No |
| Púas y lava | Fijas | No |
| Rey Babosa (jefe) | Camina, salta y lanza fuego; hay que pisarlo 3 veces | Sí (3 golpes) |

## Estructura de scripts (`Assets/Scripts`)

- `Nucleo/` Juego (vidas, puntos, cambios de nivel), Nivel (reloj), GestorAudio, SeguirCamara, Parallax
- `Jugador/` Jugador
- `Enemigos/` Enemigo (base), EnemigoCaminante, EnemigoVolador, EnemigoSaltador, Jefe
- `Objetos/` Recogible, BloqueSorpresa, Resorte, Meta, Peligro, BolaFuego, MovimientoVaiven, AnimadorSprites
- `UI/` HUD y MenuPrincipal
- `Assets/Editor/ConstruirJuego.cs`: menú **Plataformas > Construir todo**, que crea los prefabs y arma las escenas desde mapas de texto

## Créditos

- Gráficos y efectos de sonido: **New Platformer Pack** de Kenney (www.kenney.nl), licencia CC0 (dominio público).
- Música (niveles y jefe): compuesta para este proyecto, generada por código (ondas cuadradas y triangulares).
