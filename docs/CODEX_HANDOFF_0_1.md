# Codex handoff — SWARM 0.1

## Goal

Integrate the branch exactly as authored, let Unity import/configure it, verify compilation, run the toy in Editor, then generate the first Android APK. Do not redesign gameplay or architecture.

## Source

Repository: `ProtectorRudo/swarm`

Branch: `feature/swarm-0.1-toy`

Unity: `6000.3.22f1` with Android Build Support, SDK, NDK and OpenJDK installed.

## Required workflow

1. Clone or fetch the repository.
2. Checkout `feature/swarm-0.1-toy`.
3. Open the project once in Unity 6000.3.22f1 and let packages/import finish.
4. Confirm there are no C# compile errors.
5. Run `SWARM > Setup Project` if the scene was not generated automatically.
6. Enter Play Mode and verify: portrait composition, drag-to-move, pickups relocate, SWARM counter rises, followers trail behind the avatar.
7. Do not change gameplay values unless required to fix a compile/runtime blocker.
8. Build Android APK.

## Batch build

Unity must be launched with Android as the target because switching active target is not supported from batch mode.

Example Windows command (adapt Unity.exe and project paths to the machine):

```bat
"<UNITY_6000.3.22f1>\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "<PATH_TO_SWARM>" -buildTarget Android -executeMethod Swarm.Editor.SwarmBuild.BuildAndroid -logFile "<PATH_TO_SWARM>\Builds\Android\build.log"
```

Expected artifact:

`Builds/Android/SWARM-0.1.apk`

Expected success marker in log:

`SWARM_APK_READY=`

## If Unity reports a blocker

Fix only the smallest environment/API compatibility issue necessary, commit the exact fix back to the same branch with a clear message, and preserve:

- one-hand direct input;
- Simulation / Gameplay / Presentation separation;
- logical swarm count separate from visual followers;
- no per-follower Update/Animator/Rigidbody/AI.

Then rebuild.
