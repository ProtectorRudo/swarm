# SWARM

One-hand portrait mobile battle-arena prototype built with Unity.

## Product laws

1. **ONE HAND ONLY** — every gameplay-critical action must be possible with the thumb of the hand holding the phone.
2. **UNDER 20 SECONDS** — a first-time player must understand the core objective in less than 20 seconds, with no traditional tutorial flow.
3. **SIMULATION != PRESENTATION** — gameplay state never depends on the current 2D/2.5D/3D avatar representation.
4. **MOBILE FIRST** — APK/device behaviour is the product truth, not Editor behaviour.

## Current milestone

`SWARM 0.4 — Battle Arena`

Core loop under test:

`see the whole arena -> collect -> grow -> choose prey/threat -> fight automatically -> defend at home -> dominate -> survive the final rush`

Working branch: `feature/swarm-0.4-battle-arena`

Target editor: Unity `6000.3.22f1` (Unity 6.3 LTS).

Target runtime: Android, portrait, ARM64, IL2CPP, Linear color, URP 2D Renderer.

## Battle Arena scope

The complete arena is always visible. Eight armies start from eight colored bases around the perimeter: one human and seven autonomous bots. All armies compete for the same persistent food field, grow visible follower swarms, paint secondary territory, attack smaller armies automatically on contact, flee stronger armies and receive a defensive multiplier in their own territory/base.

Bots fight and eliminate each other as well as the human. There is no player-specific hunter. The final 15 seconds bias the conflict toward the resource-rich center to create a natural battle climax.

The build intentionally keeps placeholder procedural visuals, generated audio and local-only telemetry. Final characters, mutations, progression, multiplayer, shops, LiveOps, celebrations and production art remain out of scope until the battle loop passes a real phone test.

## Handoff and test docs

- `docs/CODEX_HANDOFF_0_4.md` — Unity/Codex integration and Android build procedure.
- `docs/TEST_PLAN_0_4.md` — Battle Arena phone-test acceptance criteria and metrics.
- `docs/ARCHITECTURE.md` — architectural rules.
- `docs/PRODUCT_RULES.md` — product constraints.
