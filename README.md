# SWARM

One-hand portrait mobile battle-arena prototype built with Unity.

## Product laws

1. **ONE HAND ONLY** — every gameplay-critical action must be possible with the thumb of the hand holding the phone.
2. **UNDER 20 SECONDS** — a first-time player must understand the core objective in less than 20 seconds, with no traditional tutorial flow.
3. **SIMULATION != PRESENTATION** — gameplay state never depends on the current 2D/2.5D/3D avatar representation.
4. **MOBILE FIRST** — APK/device behaviour is the product truth, not Editor behaviour.

## Current milestone

`SWARM 0.4 — Battle Arena + Crossfire`

Core loop under test:

`see whole arena -> collect -> grow -> double-tap/hold to shoot -> shrink enemy armies -> KO -> defend at home -> finish with the biggest SWARM`

Working branch: `feature/swarm-0.4-battle-arena`

Target editor: Unity `6000.3.22f1` (Unity 6.3 LTS).

Target runtime: Android, portrait, ARM64, IL2CPP, Linear color, URP 2D Renderer.

## Battle Arena scope

The complete arena is always visible. Eight armies start from eight colored bases around the perimeter: one human and seven autonomous bots. All armies compete for the same persistent food field, grow visible follower swarms, paint secondary territory and participate in visible ranged crossfire.

The human uses one thumb for everything: drag to move; make a quick tap and then hold the second tap to fire continuously. While that same second press remains held, dragging still moves and aims. There is no attack button, second stick or second finger. Mild forward aim assist keeps the control feasible on a phone without turning it into full auto-lock.

Bots use the same projectile/damage rules and target all participants symmetrically. Projectiles shrink SWARM; reaching zero causes a KO, a small reward for the shooter and respawn at the defeated army's own base. Territory/base defense can absorb part of incoming projectile pressure. The largest current SWARM wins at 60 seconds; KOs and territory only break exact ties.

The final 15 seconds activate FINAL RUSH, increasing bot aggression and biasing new food toward the center.

The build intentionally keeps placeholder procedural visuals, generated audio and local-only telemetry. Final characters, mutations, progression, multiplayer, shops, LiveOps, celebrations and production art remain out of scope until the battle loop passes a real phone test.

## Handoff and test docs

- `docs/CODEX_HANDOFF_0_4.md` — Unity/Codex integration, crossfire smoke test and Android build procedure.
- `docs/TEST_PLAN_0_4.md` — one-thumb shooting + Battle Arena phone-test criteria.
- `docs/ARCHITECTURE.md` — architectural rules.
- `docs/PRODUCT_RULES.md` — product constraints.
