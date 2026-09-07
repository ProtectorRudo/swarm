# SWARM Architecture v0

## Layers

```text
SIMULATION
  movement / swarm logical state / later territory + spatial queries
        |
        v
GAMEPLAY
  collection / rules / score / later bots + mutations
        |
        v
PRESENTATION
  avatar / swarm visuals / camera / VFX / audio / haptics / UI
```

The dependency direction is intentional. Presentation may observe gameplay; simulation must not depend on presentation.

## 0.1 decisions

- Portrait mobile.
- Direct one-finger drag input; no UI joystick as gameplay authority.
- Input produces normalized movement intent.
- Logical swarm count is independent from rendered follower proxies.
- Followers have no individual `Update`, `Animator`, `Rigidbody2D` or AI script.
- Pickups are recycled by relocation rather than destroyed/re-instantiated.
- Runtime placeholder art is procedural so the first APK has no asset-pipeline dependency.
- Initial code stays render-pipeline agnostic while URP is pinned from project creation.
- ECS is deliberately deferred; simulation data shapes are designed so Jobs/Burst can be introduced without changing game rules.

## Future avatar contract

```text
AvatarRoot (gameplay)
  Movement / Hitbox / State / Swarm owner

AvatarPresenter (replaceable)
  2D Sprite
  2D Rig
  2.5D
  3D Model + animation
```

Victory dances, zig-zags, emotes and celebrations belong to presentation definitions unless a mode explicitly makes them gameplay actions.
