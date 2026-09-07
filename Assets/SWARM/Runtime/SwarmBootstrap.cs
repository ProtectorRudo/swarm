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
            Input.multiTouchEnabled = false;

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
            var avatarPresenter = player.AddComponent<BlobPresenter>();
            avatarPresenter.Initialize(motor);

            var swarmObject = new GameObject("SwarmVisuals");
            swarmObject.transform.SetParent(root.transform, false);
            var swarm = swarmObject.AddComponent<SwarmController>();
            swarm.Initialize(player.transform, motor);
            avatarPresenter.BindSwarm(swarm);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(root.transform, false);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.AddComponent<AudioListener>();
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.068f, 0.105f, 1f);
            var cameraRig = cameraObject.AddComponent<CameraRig>();
            cameraRig.Initialize(camera, player.transform, ArenaHalfExtents);
            cameraRig.BindSwarm(swarm);

            var territoryObject = new GameObject("Territory");
            territoryObject.transform.SetParent(root.transform, false);
            var territory = territoryObject.AddComponent<TerritorySystem>();
            territory.Initialize(player.transform, ArenaHalfExtents);

            var territoryVisualObject = new GameObject("TerritoryVisual");
            territoryVisualObject.transform.SetParent(root.transform, false);
            territoryVisualObject.AddComponent<TerritoryPresenter>().Initialize(territory, ArenaHalfExtents);

            var rivalObject = new GameObject("Rival_RED");
            rivalObject.transform.SetParent(root.transform, false);
            var rival = rivalObject.AddComponent<RivalBot>();
            rival.Initialize(territory, ArenaHalfExtents);

            var hudObject = new GameObject("HUD");
            hudObject.transform.SetParent(root.transform, false);
            var hud = hudObject.AddComponent<ToyHud>();
            hud.Initialize(input, swarm);

            var matchObject = new GameObject("MatchDirector");
            matchObject.transform.SetParent(root.transform, false);
            var match = matchObject.AddComponent<MatchDirector>();
            match.Initialize(input, motor, territory, swarm, hud);
            match.BindRival(rival);

            var feedbackObject = new GameObject("FeedbackDirector");
            feedbackObject.transform.SetParent(root.transform, false);
            var feedback = feedbackObject.AddComponent<FeedbackDirector>();
            feedback.Initialize(territory, match);

            var pickupsObject = new GameObject("Pickups");
            pickupsObject.transform.SetParent(root.transform, false);
            var pickups = pickupsObject.AddComponent<PickupSystem>();
            pickups.Initialize(player.transform, swarm, avatarPresenter, cameraRig, hud, feedback, ArenaHalfExtents);

            var telemetryObject = new GameObject("FirstTestTelemetry");
            telemetryObject.transform.SetParent(root.transform, false);
            var telemetry = telemetryObject.AddComponent<FirstTestTelemetry>();
            telemetry.Initialize(match, territory, swarm);
            hud.BindTelemetry(telemetry);

            territory.CaptureCompleted += (percent, cells) =>
            {
                avatarPresenter.Pulse(1.7f);
                cameraRig.Punch(Mathf.Clamp(0.24f + cells * 0.006f, 0.28f, 0.85f));
                hud.NotifyCapture(percent, cells);
            };

            territory.TrailCut += () =>
            {
                int loss = Mathf.Max(3, Mathf.CeilToInt(swarm.Count * 0.30f));
                swarm.RemoveUnits(loss);
                motor.Teleport(Vector2.zero);
                swarm.SnapHistoryToAnchor();
                avatarPresenter.Pulse(2.2f);
                cameraRig.Punch(0.95f);
                hud.NotifyTrailCut();
            };
        }
    }
}
