# SWARM 0.4 — Battle Arena / Codex Handoff

## Source of truth

- Repository: `ProtectorRudo/swarm`
- Branch: `feature/swarm-0.4-battle-arena`
- Unity: `6000.3.22f1`
- Android modules: Android Build Support + SDK/NDK + OpenJDK
- Target APK: `Builds/Android/SWARM-0.4-battle-arena.apk`

## Why 0.4 exists

The 0.3 phone test/review exposed a presentation and fantasy problem: the follow camera hid most of the battlefield and the experience still felt like a small local encounter. 0.4 turns SWARM into a true visible free-for-all.

The entire arena must be visible at once. The player should see every colored base, every opponent and the shared food field from the first second. The game is a battle between growing armies, not a duel against one red stalker.

## Product law

The intended mental model is:

> Todos salen de su base. Juntá bichitos, hacé crecer tu ejército, comete a los más chicos y escapá de los más grandes. Tu base y tu color te ayudan a defenderte.

One thumb only. No attack button. No secondary gesture. No camera hunting. No special AI obsession with the human player.

## Intended runtime behavior

1. The complete map fits on the portrait screen at all times.
2. Eight bases are visible around the perimeter.
3. Owner 1 is the human player; owners 2-8 are seven autonomous bots.
4. All participants begin with SWARM 3 at their own base.
5. Before the human first moves: timer is stopped, bots are visible but stationary, food is visible but cannot be collected.
6. On the first real player movement all seven bots launch at the same time.
7. The field contains 132 persistent visible food particles.
8. Normal food is +1, cyan rare food is +3, pink mega food is +5.
9. High-value food is biased toward the center.
10. Every participant can collect the same food.
11. Every participant grows a visible follower swarm in its own color.
12. Every participant paints secondary influence simply by moving.
13. Painting enemy influence can cost swarm units.
14. Bots evaluate all eight participants, not the human specially.
15. Bots flee larger nearby armies, hunt smaller nearby armies and otherwise seek food.
16. Bots fight bots. Off-screen combat is impossible because the whole map is visible.
17. Contact resolves combat automatically; there is no attack button.
18. A clearly stronger army can decisively consume a weaker army and absorb part of it.
19. Close fights trade units over combat ticks until one side disengages or is defeated.
20. Fighting in your own color gives a defense bonus; fighting inside your base gives a stronger defense bonus.
21. Defeated armies respawn at their own base with SWARM 5 and short protection.
22. The final 15 seconds activate FINAL RUSH: bots become more aggressive and newly relocated food is biased toward the center.
23. HUD labels every bot with color/name/count and shows a live Top 4.
24. Match lasts 60 seconds from the first movement and ends with rank, KOs, deaths, map and final swarm.

## Integration task

Your role is integration, compile validation, smoke testing and APK generation only.

Do not redesign gameplay unless a concrete runtime defect makes the intended behavior impossible. Do not collapse this back into one red rival. Do not restore a follow camera.

1. Fetch and checkout `feature/swarm-0.4-battle-arena`.
2. Confirm the expected remote HEAD given by Mauro/ChatGPT before starting.
3. Open/import with Unity `6000.3.22f1`.
4. Allow normal Unity import/serialization.
5. Confirm zero C# compile errors.
6. Keep URP 2D, Linear color, Portrait, Android ARM64 and current project settings.
7. If new `.meta` or legitimate Unity source/config serialization is required, commit only those source-controlled artifacts to this same branch.
8. Fix only concrete compile/runtime integration blockers. Preserve the 0.4 design above.
9. Never commit `Library`, `Temp`, `Logs`, `obj`, `Builds` or caches.
10. Run the required smoke test.
11. Build with `-buildTarget Android` and `-executeMethod Swarm.Editor.SwarmBuild.BuildAndroid`.
12. Stop after generating the APK.

## Required smoke test

### Camera / arena
- Portrait launches correctly.
- Complete arena is visible without camera following the human.
- All 8 colored bases are visible simultaneously.
- Central hot zone is visible.
- Player movement never pushes another base off-screen.

### Start state
- Player starts at orange bottom base with SWARM 3.
- Seven bots are visible at seven different bases, each at SWARM 3.
- Food is already visible across the whole map.
- Before first movement: timer does not run and bots do not leave bases.
- First drag starts timer and all bots begin acting.

### Food / growth
- Approximately 132 food objects remain persistent through relocation.
- +1 / +3 / +5 food values work.
- Human magnet remains one-thumb friendly.
- Bots physically collect food.
- Each bot's follower swarm visibly grows in that bot's color.

### AI / battle
- Bots do not preferentially target the human.
- At least one bot-vs-bot combat can occur without human involvement.
- Smaller bots flee clearly stronger nearby armies.
- Stronger bots hunt reachable weaker armies.
- Bots return toward safety after losing an exchange.
- Contact combat requires no button.
- Decisive size advantage can cause an immediate KO/absorb.
- Close matchup can trade units rather than always one-shot.
- Own territory changes effective combat strength.
- Own base gives stronger defensive advantage.
- Player KO respawns at orange base with SWARM 5.
- Bot KO respawns at that bot's own base with SWARM 5.
- Respawn protection prevents instant spawn camping.

### Territory / scoring
- All 8 territory colors can appear.
- Human and bots paint while moving.
- Invading enemy influence can consume swarm units.
- Live Top 4 changes as counts/territory/KOs change.

### Final rush / result
- At 15 seconds remaining FINAL RUSH activates.
- Newly relocated food becomes more center-biased.
- Bots become more willing to hunt during final rush.
- At 0 seconds all movement/battle stops.
- Result screen shows winner, human rank out of 8, final swarm, map, KOs and deaths.
- One tap restarts the scene.

### Stability
- No recurring runtime exceptions/errors.
- Watch allocations/GC and obvious hitching with 8 armies + up to hundreds of visible followers.
- Record Editor FPS during an active crowded battle if possible.

## Build output

Expected:

`Builds/Android/SWARM-0.4-battle-arena.apk`

Expected log marker:

`SWARM_BUILD_VERSION=0.4-battle-arena`

## Final report required

Report:

- final HEAD
- zero C# errors: yes/no
- compatibility fixes made
- Unity-generated/versioned files, if any
- warnings that matter
- smoke test results item by item
- observed crowded-battle FPS if available
- exact APK path
- APK real file size
- SHA-256

Stop after the APK. The next product decision must come from Mauro's real phone test.
