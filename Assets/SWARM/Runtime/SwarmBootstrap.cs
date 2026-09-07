using UnityEngine;

namespace Swarm
{
    public static class SwarmBootstrap
    {
        private static readonly Vector2 ArenaHalfExtents = new Vector2(7.4f, 12.4f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (GameObject.Find("SWARM_ROOT") != null) return;

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.orientation = ScreenOrientation.Portrait;

            var root = new GameObject("SWARM_ROOT");

            var arenaObject = new GameObject("Arena");
            arenaObject.transform.SetParent(root.transform, false);
            arenaObject.AddComponent<ArenaPresenter>().Build(ArenaHalfExtents);

            var inputObject = new GameObject("OneHandInput");
            inputObject.transform.SetParent(root.transform, false);
            var input = inputObject.AddComponent<OneHandInputSource>();

            var player = new GameObject("AvatarRoot");
            player.transform.SetParent(root.transform, false);
            player.transform.position = Vector3.zero;
            var motor = player.AddComponent<PlayerMotor>();
            motor.Initialize(input, ArenaHalfExtents);
            var presenter = player.AddComponent<BlobPresenter>();
            presenter.Initialize(motor);

            var swarmObject = new GameObject("SwarmVisuals");
            swarmObject.transform.SetParent(root.transform, false);
            var swarm = swarmObject.AddComponent<SwarmController>();
            swarm.Initialize(player.transform, motor);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(root.transform, false);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.068f, 0.105f, 1f);
            var cameraRig = cameraObject.AddComponent<CameraRig>();
            cameraRig.Initialize(camera, player.transform);

            var hudObject = new GameObject("HUD");
            hudObject.transform.SetParent(root.transform, false);
            var hud = hudObject.AddComponent<ToyHud>();
            hud.Initialize(input, swarm);

            var pickupsObject = new GameObject("Pickups");
            pickupsObject.transform.SetParent(root.transform, false);
            var pickups = pickupsObject.AddComponent<PickupSystem>();
            pickups.Initialize(player.transform, swarm, presenter, cameraRig, hud, ArenaHalfExtents);
        }
    }
}
