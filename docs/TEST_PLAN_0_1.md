# SWARM 0.1 — first APK test plan

The build is a toy, not a finished game. We are testing feel.

## Hard constraints

- Use the phone with one hand only.
- Do not use the second hand to stabilize or operate controls.
- A new player should understand `move -> collect -> grow` in under 20 seconds.

## Test 1 — first 20 seconds

Without instructions from the developer, launch the APK and play.

Record:

- seconds until movement is understood;
- seconds until collection is understood;
- seconds until growth/followers are understood;
- any moment the player looks for a second control/button.

## Test 2 — movement feel

Try:

- small circles;
- sudden 180-degree direction changes;
- long diagonal drags;
- lifting and replacing the thumb;
- playing near every edge of the reachable thumb area.

Judge: immediate / heavy / slippery / nervous / comfortable.

## Test 3 — collection feel

Collect at least 30 units. Judge whether each pickup feels noticeable and whether the visual growth creates a desire to collect one more.

## Test 4 — performance sanity

Play for two minutes. Note visible stutter, heat, frozen input, follower jitter or FPS collapse.

## What feedback is most useful

Examples:

- `turning feels late`;
- `I understood movement instantly but not what to collect`;
- `growth feels satisfying until 12, then I stop noticing it`;
- `the tail is too long`;
- `I want to keep collecting`;
- `I got bored after 20 seconds`.
