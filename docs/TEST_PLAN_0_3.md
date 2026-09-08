# SWARM 0.3 — Phone Test Plan

## What this test answers

0.3 is not an art test. It answers whether the new core is understandable and more fun than the 0.2 Paper.io-like loop.

Core under test:

**JUNTAR -> CRECER -> PINTAR -> PELEAR/COMER -> HUIR/DEFENDER -> REPETIR**

## Hard rules

- Phone in one hand only.
- Same thumb performs the entire match.
- Do not stabilize with the second hand.
- Do not receive any explanation while playing.
- Play at least 3 full matches.

## Pass criteria

Within the first 20 seconds, preferably under 10, the player should discover without outside help that:

1. moving is drag with one thumb;
2. pickups increase SWARM;
3. moving automatically expands owned territory;
4. a larger SWARM has visibly greater presence/power;
5. touching the red rival is combat;
6. if the rival is clearly larger, avoiding it or retreating to owned territory makes sense.

The player should never ask how to close a loop or how to activate an attack button, because neither exists.

## What to record after each match

Take a screenshot of the result screen. Record:

- PRIMER CRECIMIENTO
- EXPANSIONES
- ROJAS ROBADAS
- PELEAS +
- PELEAS -
- MÁX SWARM
- player map % vs red map %

Also report in plain language:

- Did movement feel natural with one thumb?
- Did collecting feel satisfying?
- Could you tell that a bigger swarm painted more?
- Did you understand that combat happened by touching?
- Could you predict whether you should attack or flee?
- Did own-territory defense make sense from play, not from explanation?
- Did the red bot feel fair or like it was camping you?
- At what exact moment, if any, did boredom appear?
- Did you voluntarily want another match?

## Immediate failure conditions

Iterate the core before adding features if any of these happen repeatedly:

- player does not understand growth/power relationship in <20 s;
- player still asks how to attack;
- territory expansion is not visually obvious;
- red bot repeatedly camps or chain-kills the player;
- player cannot find enough pickups to recover;
- combat outcome feels arbitrary despite visible counts;
- one-hand control becomes uncomfortable;
- no spontaneous desire for a second/third match.

## Out of scope

Do not judge final commercial quality from this build. Still out of scope:

- final characters/3D rigs
- skins/emotes/dances
- mutations/skills
- multiple polished bots
- progression/meta
- shop/monetization
- multiplayer
- social/rivalry systems
- final VFX/audio/art direction

0.3 must prove the new core before any of those return to scope.
