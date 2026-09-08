# SWARM 0.3 — Core Rework / Codex Handoff

## Source of truth

- Repository: `ProtectorRudo/swarm`
- Branch: `feature/swarm-0.3-core-rework`
- Unity: `6000.3.22f1`
- Android modules required: Android Build Support + SDK/NDK + OpenJDK
- Target APK: `Builds/Android/SWARM-0.3-core-rework.apk`

## Why this branch exists

The first phone test of 0.2 proved that movement and collection were understandable, but the Paper.io-style loop closure and trail-cut interaction were not. The red bot also felt like it camped the player. 0.3 deliberately replaces that core instead of polishing it.

## Product law for 0.3

The player should understand this without external explanation:

> Juntá bichitos, hacé crecer tu ejército, pintá mapa al moverte y comete rivales más chicos. Si uno es más grande, crecés o te defendés en tu color.

One thumb only. No attack button. No second gesture. No exposed trail. No loop closure.

## Intended runtime behavior

1. Player starts with SWARM 3.
2. Match, pickups and rival remain dormant until the first real movement.
3. Yellow/cyan pickups grow the player swarm; food density is intentionally higher than 0.2.
4. Player territory paints automatically while moving.
5. Larger swarm = wider territory brush.
6. Red rival also has a visible swarm count and visible followers.
7. Red rival physically collects food and paints red territory.
8. Converting enemy territory costs swarm units.
9. Combat is automatic when player and rival touch.
10. Effective combat strength is based on swarm size, with a defensive multiplier when fighting on your own territory.
11. Stronger side damages/absorbs the weaker side. Defeated player respawns with 5; defeated rival respawns away from the player.
12. Rival has an onboarding peace window and anti-camping logic. It should not sit on the player waiting to punish every move.
13. HUD explicitly teaches collect -> grow -> paint -> eat/flee.
14. Match remains 60 seconds after first movement.

## Integration task

Do not redesign the game. Your task is compile/integration/smoke/build only.

1. Fetch and checkout `feature/swarm-0.3-core-rework`.
2. Confirm the branch is clean before local compatibility fixes.
3. Open/import with Unity `6000.3.22f1`.
4. Allow normal Unity serialization/import if required.
5. Confirm zero C# compile errors.
6. Keep URP 2D, Linear color space, Portrait, Android ARM64 and current project settings.
7. Fix only concrete compile/runtime integration blockers. Do not restore the old trail/loop system.
8. If Unity legitimately rewrites source-controlled config/meta assets, commit only required source/config changes to this same branch.
9. Never commit `Library`, `Temp`, `Logs`, `obj`, `Builds` or caches.
10. Run the 0.3 smoke test below.
11. Build the APK using `-buildTarget Android` and `-executeMethod Swarm.Editor.SwarmBuild.BuildAndroid`.
12. Stop after the APK. Do not add features.

## Required smoke test

- Portrait launches correctly.
- 60 FPS target remains configured.
- Before first movement: timer does not run, rival does not roam/paint, pickups do not auto-collect.
- One pointer/one thumb moves the avatar.
- Player starts at SWARM 3.
- First nearby pickups can be collected quickly.
- Player followers visibly increase as SWARM grows.
- Player paints territory merely by moving; no loop closure is required.
- Territory brush becomes visibly wider at larger swarm sizes.
- Red territory exists and red rival paints while moving.
- Rival has a visible `ROJO N` label and visible red followers.
- Rival collects pickups when physically reaching them.
- Invading red territory converts it to player territory and can consume some swarm.
- Invading player territory costs the rival some swarm.
- Touching the rival triggers automatic combat; there is no attack button.
- Bigger swarm normally wins exchanges.
- Own-territory defensive bonus changes at least one close matchup as intended.
- Player defeat resets player to SWARM 5 and home without scene reload.
- Rival defeat respawns rival away with its reset swarm.
- Rival does not permanently camp the player after respawn.
- HUD messages correspond to actual current strength/territory.
- 60 second end screen works.
- One tap restarts.
- No recurring Console exceptions/errors.

## Build output

Expected:

`Builds/Android/SWARM-0.3-core-rework.apk`

Expected log marker:

`SWARM_BUILD_VERSION=0.3-core-rework`

## Final report required from Codex

Report:

- final HEAD
- zero C# errors: yes/no
- any compatibility fixes made
- Unity-generated/versioned files, if any
- warnings that matter
- smoke-test results, item by item
- observed Editor FPS if available
- exact APK path
- APK size
- SHA-256 if convenient

Do not continue development after producing the APK. The next product decision comes from the real phone test.
