# SWARM 0.4 — Battle Arena Phone Test

## Purpose

0.4 is not an art-quality test. It tests whether seeing the entire battlefield and fighting in a true eight-army free-for-all creates a stronger, clearer and more exciting core than the previous close-camera versions.

## Non-negotiable setup

- Real Android phone.
- Portrait.
- Hold the phone with one hand only.
- Do not support the phone with the second hand.
- Do not explain the rules before the first match.
- Play at least 5 matches because emergent bot-vs-bot situations vary.

## Under-20-second comprehension test

Without prior explanation, the player should understand within 20 seconds that:

1. The colored circles around the edge are bases/starting armies.
2. Yellow/cyan/pink particles make SWARM grow.
3. Every colored army is doing the same thing.
4. A visibly larger-number army is dangerous to a smaller-number army.
5. Contact is the attack; there is no attack button.
6. Returning toward your color/base is a defensive option.
7. The goal is to finish with the largest SWARM in the arena.

Failure on any of these means the core presentation/onboarding needs revision before adding content.

## Match-by-match observations

For each match record:

- Did the full map feel readable or too tiny/crowded?
- Could you immediately locate your orange base/player?
- Could you tell which armies were becoming strong without searching for them?
- Did the live Top 4 make sense just by comparing SWARM counts?
- Did bot-vs-bot battles happen visibly?
- Did any bot feel unfairly obsessed with you?
- Did you consciously choose at least once to hunt a smaller rival?
- Did you consciously choose at least once to flee a larger rival?
- Did returning to your base ever save you?
- Was food density satisfying or visually noisy?
- Did high-value center food tempt you into risk?
- Did the last 15 seconds feel more intense?
- Exact second, if any, where boredom started.
- Did you want to tap for another battle when the result appeared?

## Result-screen metrics

Capture screenshots showing:

- rank /8
- final SWARM
- map %
- KOs
- deaths
- time to SWARM 10
- time to first combat
- max SWARM

## Acceptance targets

### Clarity
- Core mental model understood in <=20 seconds.
- Preferred: player understands food/growth within 8 seconds, size-based danger within 15 seconds, and knows that the largest final SWARM wins without external explanation.

### Agency
- Player can intentionally pursue at least one smaller target by match 2.
- Player can intentionally escape/return home from a larger threat by match 2.

### Emergence
- At least one obvious bot-vs-bot KO in most matches.
- No bot consistently follows the human for long stretches without a size/position reason.
- Leader can change during a match because army sizes change.

### Excitement
- At least one moment per match should create a clear reaction: chase, escape, KO, being cornered, stealing high-value center food, or watching two large armies collide.
- Final 15 seconds should feel materially more dangerous than the opening.

### Replay
- After 3 matches, the player should still want to test a different risk/route/target.
- If replay desire is absent, do not add skins, progression, mutations or more bots. Fix battle pacing first.

### Performance
- Target 60 FPS on device.
- No repeated hitches when several armies exceed SWARM 30.
- No input lag or missed thumb movement.

## Failure interpretation

- **Map too small/readability fails:** tune arena scale, icon/follower scale, HUD labels or camera margin; do not return to follow camera by default.
- **Too chaotic immediately:** reduce early food value/AI aggression or add a slightly longer opening farm phase, while keeping all armies visible.
- **Too calm:** increase center value, hunt willingness or final-rush pressure.
- **Player gets dogpiled:** audit target scoring; bots must treat all participants symmetrically.
- **Combat outcome unclear:** improve number/impact feedback before adding mechanics.
- **Base defense invisible:** strengthen base visual/feedback or simplify the bonus rule.
- **Ranking unclear:** keep ranking tied to SWARM size; do not add hidden scoring complexity.
- **No replay urge:** the battle loop itself failed; do not mask it with metagame systems.

## Out of scope for this test

- final character art
- mutations/skills
- skins
- progression
- shop
- ads/IAP
- multiplayer
- seasons
- social systems
- final VFX/audio
- 3D avatars
- dances/celebrations

Those come only after the free-for-all core proves itself on a real phone.
