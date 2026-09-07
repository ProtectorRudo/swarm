# SWARM Product Rules

These rules are requirements, not preferences.

## 1. One hand only

The complete core match must be playable with one thumb while the same hand holds the phone. No left-stick/right-button layout, no two-finger gestures, and no mechanic may require a second hand.

## 2. Under 20 seconds

The first match must teach the core by interaction. Target comprehension is 10 seconds; 20 seconds is the hard ceiling.

The opening loop is:

`DRAG -> MOVE -> TOUCH PICKUP -> GROW`

Later systems must preserve this clarity.

## 3. Input is intent

Touch code emits a `MoveIntent`; it never writes player transform position directly. The same movement simulation must later accept human input, bot intent, replay data, or network intent.

## 4. Simulation and presentation are separate

The avatar's hitbox, position, ownership and gameplay state do not live in the visual prefab. A future `Avatar3DPresenter` must be able to replace the initial 2D presenter without rewriting movement, territory, combat or bots.

## 5. Mobile-first performance

Gameplay must be profiled on Android early. Logical swarm size and visible follower count are separate concepts. Thousands of logical units must never imply thousands of animated GameObjects.

## 6. No feature earns a place by complexity

A feature survives only if it improves clarity, feel, rivalry, mastery, expression or shareability.
