# SWARM 0.4 — Battle Arena + Crossfire Phone Test

## Purpose

This phone test validates two things together:

1. whether the full visible eight-army arena is more exciting/readable than the old close camera;
2. whether one-thumb double-tap-and-hold shooting is intuitive enough to become SWARM's primary combat language.

This is not an art-quality test.

## Non-negotiable setup

- Real Android phone.
- Portrait.
- Hold the phone with one hand only.
- Do not support the phone with the second hand.
- No explanation before match 1 beyond whatever the game itself shows.
- Play at least 5 matches.

## Core mental model target

Within 20 seconds the player should understand:

1. every colored group is an army starting from its own base;
2. food increases SWARM;
3. drag moves;
4. double tap + hold fires with the same thumb;
5. dragging while holding fire still moves/aims;
6. bullets reduce the other army's SWARM;
7. when an army reaches zero it is KO'd;
8. own color/base provides defensive value;
9. at match end the biggest SWARM wins.

Preferred targets:

- understand food/growth within 8 seconds;
- discover or follow the shooting prompt within 15 seconds;
- intentionally hit another army within 20 seconds.

## Match observations

For every match record:

- Could you immediately find your orange player/base?
- Was the complete arena readable or too small/crowded?
- Could you see all other armies doing things without moving the camera?
- Did the first double-tap-and-hold fire attempt work naturally?
- Did you accidentally fire when you only wanted to move?
- Did firing ever cancel or damage normal movement control?
- While firing, could you still steer comfortably with the same thumb?
- Did bullets go where you expected?
- Was the mild aim assist helpful or did it feel like the game stole your aim?
- Could you clearly see an enemy count shrink after hits?
- Did you understand defended/blocked shots near territory/base?
- Did bots visibly shoot each other?
- Did you witness a bot-vs-bot KO?
- Did any bot still feel obsessed with you?
- Did you intentionally shoot a smaller rival?
- Did you intentionally shoot while escaping a larger rival?
- Did your base ever save you?
- Did projectile density feel exciting or visually noisy?
- Did FINAL RUSH feel more intense?
- Exact second, if any, where boredom started.
- Did you want another match immediately?

## Result-screen metrics

Capture screenshots showing as much as possible of:

- rank /8
- final SWARM
- map %
- KOs
- deaths
- shots fired
- hits
- time to first shot
- time to SWARM 10
- max SWARM

## Acceptance targets

### Control

- One hand only for the full match.
- Normal drag remains comfortable.
- Double-tap-and-hold succeeds reliably after learning it once.
- Preferred: successful fire activation on at least 8/10 deliberate attempts.
- Accidental fire activations should be rare.
- Releasing the hold must stop fire immediately.
- Moving while firing must not require changing grip.

### Combat clarity

- Player understands that bullets shrink enemy SWARM without external explanation.
- Rival label/follower loss makes damage visible.
- Player can deliberately damage a chosen opponent by match 2.
- Player can recognize when own/base defense absorbed some pressure.
- KO cause should be obvious.

### Free-for-all emergence

- Bot-vs-bot shooting happens in most matches.
- Bot-vs-bot KO should happen in most sufficiently active matches.
- No bot should consistently prefer the human without a symmetric size/distance reason.
- Multiple independent fights may happen at once.
- Leader can change during the match.

### Excitement

At least one strong reaction per match should come from:

- sustained crossfire;
- chasing a damaged enemy;
- escaping while shooting backward/along movement;
- getting caught between two armies;
- bot-vs-bot KO nearby;
- stealing valuable center food during a firefight;
- surviving at base;
- a late FINAL RUSH comeback.

### Replay

After 3 matches, player should still want to try a different route/target/risk.
If not, do not add skins, progression, mutations or more players. Fix the battle loop first.

### Performance

- Target 60 FPS on device.
- No repeated hitching during sustained eight-way fire.
- No missed input while many projectiles are active.
- No obvious GC spikes from shooting.

## Failure interpretation

- **Double tap feels awkward:** tune timing/distance thresholds first; do not immediately add a second button.
- **Accidental firing:** tighten first-tap duration/travel or double-tap spatial window.
- **Hard to aim:** tune forward aim-assist cone/range before adding independent aim controls.
- **Can't move while firing:** input design failed and must be fixed before any feature expansion.
- **Bullets too tiny/noisy:** tune projectile scale/speed/fire cadence, not camera follow.
- **Armies die too fast:** slow fire cadence, add defensive pressure, or increase early growth time.
- **Armies never die:** raise projectile pressure or reduce defensive absorption.
- **Bots dogpile human:** audit symmetric targeting.
- **Map unreadable:** tune arena/player/follower/HUD scales while preserving full-map camera as default.
- **No replay urge:** battle pacing/core failed; metagame will not fix it.

## Out of scope

- final characters
- mutations/skills
- skins
- progression
- shop
- ads/IAP
- multiplayer
- seasons/social
- production VFX/audio
- 3D avatars
- dances/celebrations

Those come only after full-map crossfire proves itself on a real phone.
