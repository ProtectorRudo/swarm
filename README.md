# SWARM

One-hand portrait mobile arena game prototype built with Unity.

## Product laws

1. **ONE HAND ONLY** — every gameplay-critical action must be possible with the thumb of the hand holding the phone.
2. **UNDER 20 SECONDS** — a first-time player must understand the core objective in less than 20 seconds, with no traditional tutorial flow.
3. **SIMULATION != PRESENTATION** — gameplay state never depends on the current 2D/2.5D/3D avatar representation.
4. **MOBILE FIRST** — APK/device behaviour is the product truth, not Editor behaviour.

## Current milestone

`SWARM 0.2 — First Test Slice`

Core loop under test:

`move -> collect -> grow -> leave territory -> expose trail -> close capture -> avoid rival cut -> score -> replay`

Working branch: `feature/swarm-0.2-first-test`

Target editor: Unity `6000.3.22f1` (Unity 6.3 LTS).

Target runtime: Android, portrait, ARM64, IL2CPP, Linear color, URP 2D Renderer.

## First-test scope

The build includes direct one-pointer movement, pickups and swarm growth, logical/visual follower separation, grid territory capture, one lightweight trail-hunting rival, a 60-second match loop, one-tap replay, and local first-test telemetry.

It intentionally does **not** yet include final character art, mutations, multiple rivals, real progression, multiplayer, 3D avatars, dances, celebrations, shops, or LiveOps.

## Handoff and test docs

- `docs/CODEX_HANDOFF_0_2.md` — Unity/Codex integration and Android build procedure.
- `docs/TEST_PLAN_0_2.md` — phone-test acceptance criteria and metrics.
- `docs/ARCHITECTURE.md` — architectural rules.
- `docs/PRODUCT_RULES.md` — product constraints.
