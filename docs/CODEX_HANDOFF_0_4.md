# SWARM 0.4 — Battle Arena + Crossfire / Codex Handoff

## Source of truth

- Repository: `ProtectorRudo/swarm`
- Branch: `feature/swarm-0.4-battle-arena`
- Unity: `6000.3.22f1`
- Android modules: Android Build Support + SDK/NDK + OpenJDK
- Target APK: `Builds/Android/SWARM-0.4-battle-arena.apk`

## Product change in this revision

0.4 keeps the full-map eight-army battle arena, but direct contact is no longer the primary combat mechanic.

The new combat experiment is ranged:

> Drag with one thumb to move. Make a quick first tap, then press-and-hold the second tap to fire continuously with that same thumb. While holding fire, dragging still moves and aims. Projectiles shrink the enemy SWARM. At zero, that army is KO'd and respawns at its base.

There is no attack button, second stick, second finger or separate aim gesture.

## Product law

The intended mental model is:

> Todos salen de su base. Juntá bichitos, hacé crecer tu ejército, disparale a los demás para achicarles el SWARM y escapá cuando te superan. Tu base y tu color ayudan a defenderte. Al final gana el SWARM más grande.

## Intended runtime behavior

1. Entire portrait arena remains visible at once.
2. Eight colored bases are visible around the perimeter.
3. Owner 1 is the human; owners 2-8 are seven autonomous bots.
4. Everyone starts at SWARM 3 in their own base.
5. Before the first real human movement: timer is stopped, bots are stationary and food is visible but inactive.
6. First movement starts all seven bots, food collection, territory paint and the 60-second timer.
7. 132 persistent visible food particles exist; normal +1, cyan +3, pink +5.
8. Every participant can collect the same field and visibly grow followers.
9. Every participant paints secondary influence while moving.
10. Complete camera stays fixed to the arena; it never follows the human.
11. The human fires using one-thumb double-tap-and-hold.
12. The second press may continue dragging: movement and firing coexist with one finger.
13. Player bullets use current/last movement direction with mild forward aim assist; there is no auto-lock behind the player.
14. All projectiles are centrally pooled/simulated; projectile GameObjects have no individual Update.
15. Bots use the same bullets and damage rules as the player.
16. Bots choose ranged targets symmetrically among all participants; they do not prefer the human.
17. Bots can shoot other bots while the human is elsewhere.
18. One damaging projectile removes SWARM units. Own territory/base can absorb part of projectile pressure through the defense multiplier.
19. Visible rival labels update immediately as bullets shrink armies.
20. KO gives the shooter a small +3 SWARM reward.
21. Defeated armies respawn in their own base with SWARM 5 and short invulnerability.
22. Bots under fire can retreat, especially when small.
23. Final 15 seconds activate FINAL RUSH: more aggressive bot firing/hunting and center-biased food relocation.
24. Top 4 is based primarily on current SWARM.
25. At 0 seconds the army with the largest current SWARM wins; KOs/territory only break exact ties.

## Integration task

Your role is integration, compile validation, smoke testing and APK generation only.

Do not redesign the controls or combat. Do not replace double-tap-and-hold with a button, second stick or second finger. Do not restore contact combat as the main damage mechanic. Do not restore a follow camera. Do not collapse the arena to one red rival.

1. Fetch and checkout `feature/swarm-0.4-battle-arena`.
2. Confirm the expected remote HEAD supplied in the handoff message before starting.
3. Open/import with Unity `6000.3.22f1`.
4. Allow normal Unity serialization/import.
5. Confirm zero C# compile errors.
6. Keep URP 2D, Linear color, Portrait, Android ARM64 and current project settings.
7. If Unity legitimately creates/changes `.meta` or source-controlled config, commit only the required artifacts to the same branch.
8. Fix only concrete compile/runtime blockers. Preserve the design above.
9. Never commit `Library`, `Temp`, `Logs`, `obj`, `Builds` or caches.
10. Run the smoke test below.
11. Build with `-buildTarget Android` and `-executeMethod Swarm.Editor.SwarmBuild.BuildAndroid`.
12. Stop after the APK.

## Required smoke test

### Camera / arena
- Portrait launches correctly.
- Complete arena is visible without camera following the human.
- All 8 colored bases are visible simultaneously.
- Central hot zone and shared food field are visible.
- Moving to any arena edge never pushes another base off-screen.

### Start state
- Human starts at orange base with SWARM 3.
- Seven bots are visible at their own bases with SWARM 3.
- Food is visible before movement.
- Before movement: timer stopped, bots stationary, no projectiles active and food cannot be collected.
- First drag starts timer, bots, food and battle simulation.

### One-thumb fire gesture
- Normal drag still moves exactly as before.
- A quick tap followed by second press-and-hold sets `FireHeld`.
- Releasing the second press stops firing.
- While that second press remains held, dragging still moves the player.
- Fire direction follows movement/last-facing direction.
- Mild forward aim assist can pull toward a target in front, but does not target enemies behind the player.
- No second finger is needed.
- No attack UI button exists.
- Editor Space fallback may exist only for automated/editor validation; phone truth is the one-thumb gesture.

### Projectiles
- Player bullets are visibly orange.
- Every bot fires bullets in its own owner color.
- Projectile pool is created once and reused; no instantiate/destroy loop during steady-state firing.
- Crossfire from multiple armies can coexist.
- Bullets expire or leave arena cleanly.
- A projectile cannot damage its shooter.
- Respawn-invulnerable targets cannot be damaged.
- Damaging hits visibly reduce the target's SWARM count/followers.
- Own territory/base defense can absorb some projectile pressure deterministically.
- HUD differentiates hit, incoming hit and defended impact for the human.

### AI / free-for-all
- Bots do not preferentially target the human.
- At least one bot-vs-bot ranged exchange occurs without human involvement.
- At least one bot can KO another bot with projectiles.
- Smaller bots flee strong nearby armies.
- Strong bots may hunt weaker armies.
- Small bots under fire can retreat toward safety.
- Bots still collect food and paint influence while fighting.

### KO / scoring
- Projectile damage can reduce an army to zero and trigger KO.
- Shooter receives +3 SWARM on KO.
- Human KO respawns at orange base with SWARM 5 and protection.
- Bot KO respawns at its own base with SWARM 5 and protection.
- No immediate spawn-kill loop occurs.
- Top 4 updates as projectile damage/growth change current SWARM.
- Largest current SWARM is the winner at match end.

### Final rush / result
- At 15 seconds remaining FINAL RUSH activates.
- Bots fire/hunt more aggressively.
- Relocated food becomes more center-biased.
- At 0 seconds movement/battle/projectiles stop.
- Result screen shows winner, rank /8, SWARM, map, KOs/deaths, shots and hits.
- One tap restarts.

### Stability/performance
- No recurring runtime exceptions/errors.
- Watch GC/allocation behavior during sustained eight-way crossfire.
- Record crowded-battle FPS if possible.
- Specifically stress many active bullets + 8 armies + followers + 132 pickups simultaneously.

## Build output

Expected:

`Builds/Android/SWARM-0.4-battle-arena.apk`

Expected marker:

`SWARM_BUILD_VERSION=0.4-battle-arena`

## Final report required

Report:

- final HEAD
- zero C# errors: yes/no
- compatibility fixes made
- Unity-generated/versioned files, if any
- warnings that matter
- smoke test item by item
- proof/result of double-tap-hold fire test
- proof/result that movement still works while firing
- proof/result of bot-vs-bot ranged combat and bot-vs-bot KO
- all 8 bases visible simultaneously: yes/no
- crowded crossfire FPS if available
- max active projectiles observed if practical
- exact APK path
- real APK size
- SHA-256

Stop after generating the APK. The next design decision comes from Mauro's real phone test.
