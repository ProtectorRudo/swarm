# SWARM 0.2 — First phone test plan

## Purpose

This test asks one question: does the smallest complete SWARM loop create enough clarity, control, tension, and desire for another match to justify deeper production?

This is not an art-quality test.

## Non-negotiable acceptance gates

### One hand only

Hold the phone in one hand for the entire session. Do not support it or interact with the other hand.

Fail if any gameplay action feels like it wants a second hand, a second finger, or an unreachable button.

### Understand in under 20 seconds

On the first-ever match, do not read external instructions. The in-game guidance must be enough.

Target: first successful territory capture <= 20 seconds after first movement.
Preferred: <= 12 seconds.

The result screen reports `PRIMERA CAPTURA` for this reason.

### Control feel

Movement must feel immediate and predictable. Watch for:

- delay after thumb direction changes;
- uncomfortable drag distance;
- excessive dead zone;
- avatar feeling heavy or nervous;
- thumb obscuring important action;
- inability to make a clean loop.

### Reward feel

Collect several normal pickups and cyan bonus pickups.

Judge whether:

- magnetism feels helpful rather than automatic;
- follower growth is readable;
- the avatar visibly feels stronger as swarm grows;
- collection makes you want to seek the next pickup.

### Territory feel

Make several loops of different sizes.

Judge whether:

- leaving safety is visually obvious;
- closing the loop is understood without explanation;
- capture feedback feels satisfying;
- AREA % provides a clear goal;
- larger loops create more excitement than tiny loops.

### Threat feel

The first capture must be safe. After that, expose a trail near the red rival.

Judge whether:

- it is obvious that red is dangerous;
- being cut feels fair/readable;
- the loss creates tension without feeling arbitrary;
- you immediately understand why you failed;
- you want revenge rather than wanting to quit.

### Replay desire

Finish at least three matches.

After each result screen, record the first instinct:

- `otra ya`;
- `otra pero cambiaría X`;
- `ya entendí, no quiero otra`.

The strongest success signal is voluntarily tapping replay before thinking about it.

## Metrics to report after each match

Take a screenshot of the result screen or report:

- AREA %;
- SWARM final;
- PRIMERA CAPTURA;
- CAPTURAS;
- CORTES;
- MAX SWARM;
- approximate FPS shown during play.

## First-test failure conditions

Stop adding features and iterate the core if any of these happen:

- first capture consistently takes >20 seconds;
- control is uncomfortable with one hand;
- the player cannot explain the objective after one match;
- territory capture is confusing;
- red rival feels random/unfair;
- collecting does not feel rewarding;
- there is no desire to replay after 2–3 matches;
- phone performance is visibly unstable.

## Explicitly out of scope

Do not judge this build on final character art, 3D avatars, dances, celebrations, mutations, full bot roster, progression, shop, multiplayer, seasons, social systems, or final audio/VFX. Those come only after the core test produces a reason to continue.
