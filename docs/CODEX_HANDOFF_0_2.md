# Codex handoff — SWARM 0.2 first test

## Goal

Integrate the first meaningful phone-test slice exactly as authored, verify Unity/Android compatibility, run a smoke test, and generate the APK. Do not redesign gameplay or architecture.

## Source

- Repository: `ProtectorRudo/swarm`
- Branch: `feature/swarm-0.2-first-test`
- Unity: `6000.3.22f1`
- Required modules: Android Build Support + SDK + NDK + OpenJDK

## What this build validates

This is not a commercial-art test. It validates the core loop:

`move -> collect -> grow -> leave territory -> expose trail -> close capture -> avoid rival cut -> score -> replay`

Non-negotiable product laws:

1. Entire gameplay must be usable with one hand / one thumb.
2. First-time player should understand the core in under 20 seconds.
3. Gameplay logic must remain independent from 2D/3D presentation.
4. No per-follower `Update`, `Animator`, `Rigidbody`, or AI.

## Required workflow

1. Clone/fetch the repository.
2. Checkout `feature/swarm-0.2-first-test`.
3. Verify HEAD against the commit supplied in the handoff message from ChatGPT.
4. Open with Unity `6000.3.22f1`.
5. Let package import and project auto-setup finish.
6. Confirm the generated project uses URP with the 2D Renderer and Linear color space.
7. Confirm there are zero C# compile errors.
8. If needed, run `SWARM > Setup Project` once.
9. Enter Play Mode and perform the smoke test below.
10. Fix only the smallest technical blocker needed for compile/runtime. Do not rebalance or redesign by preference.
11. Commit any required integration fix back to the same branch with a clear message.
12. Build Android APK.

## Smoke test

Verify in Play Mode:

- portrait orientation;
- drag with one pointer moves the avatar;
- releasing stops/decelerates movement correctly;
- pickups magnetize at short range and increase SWARM count;
- cyan bonus pickups add more mass;
- visible followers trail the avatar;
- avatar grows visually without changing gameplay hitbox logic;
- initial territory appears around spawn;
- leaving owned territory paints an exposed trail;
- re-entering owned territory closes a capture and increases AREA %;
- first capture is safe from rival punishment;
- after first capture the red rival can hunt/cut exposed trails;
- a cut removes exposed trail, costs swarm mass, and resets player home;
- match timer begins only after first movement;
- match ends after 60 seconds;
- result screen appears;
- one pointer tap restarts;
- result screen reports first-capture time, captures, cuts, and max swarm;
- no recurring exceptions/errors in Console.

## Build command

Unity must launch with Android already selected because `SwitchActiveBuildTarget` is unsupported in batch mode.

Example Windows command (adapt paths):

```bat
"<UNITY_6000.3.22f1>\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "<PATH_TO_SWARM>" -buildTarget Android -executeMethod Swarm.Editor.SwarmBuild.BuildAndroid -logFile "<PATH_TO_SWARM>\Builds\Android\build.log"
```

Expected artifact:

`Builds/Android/SWARM-0.2-first-test.apk`

Expected log markers:

- `SWARM_APK_READY=`
- `SWARM_APK_SIZE_BYTES=`
- `SWARM_BUILD_VERSION=0.2-first-test`

## Report back

Return:

- final commit SHA;
- whether Unity compiled with zero C# errors;
- technical changes required, if any;
- APK path;
- APK size;
- relevant warnings/errors;
- observed Play Mode FPS if measurable;
- whether every smoke-test item passed.

Do not implement mutations, additional bots, skins, real characters, multiplayer, dances, shops, or new gameplay during this handoff.
