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
            swarm.SetCount(3);
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
            territory.Initialize(player.transform, swarm, ArenaHalfExtents);

            var territoryVisualObject = new GameObject("TerritoryVisual");
            territoryVisualObject.transform.SetParent(root.transform, false);
            territoryVisualObject.AddComponent<TerritoryPresenter>().Initialize(territory, ArenaHalfExtents);

            var rivalObject = new GameObject("Rival_RED");
            rivalObject.transform.SetParent(root.transform, false);
            var rival = rivalObject.AddComponent<RivalBot>();
            rival.Initialize(territory, ArenaHalfExtents, player.transform, motor, swarm);

            var hudObject = new GameObject("HUD");
            hudObject.transform.SetParent(root.transform, false);
            var hud = hudObject.AddComponent<ToyHud>();
            hud.Initialize(input, swarm);
            hud.BindRival(rival);

            var matchObject = new GameObject("MatchDirector");
            matchObject.transform.SetParent(root.transform, false);
            var match = matchObject.AddComponent<MatchDirector>();
            match.Initialize(input, motor, territory, swarm, hud);
            match.BindRival(rival);

            var feedbackObject = new GameObject("FeedbackDirector");
            feedbackObject.transform.SetParent(root.transform, false);
            var feedback = feedbackObject.AddComponent<FeedbackDirector>();
            feedback.Initialize(territory, match, rival);

            var pickupsObject = new GameObject("Pickups");
            pickupsObject.transform.SetParent(root.transform, false);
            var pickups = pickupsObject.AddComponent<PickupSystem>();
            pickups.Initialize(player.transform, swarm, rival, avatarPresenter, cameraRig, hud, feedback, ArenaHalfExtents);
            match.BindPickups(pickups);

            var telemetryObject = new GameObject("FirstTestTelemetry");
            telemetryObject.transform.SetParent(root.transform, false);
            var telemetry = telemetryObject.AddComponent<FirstTestTelemetry>();
            telemetry.Initialize(match, territory, swarm, rival);
            hud.BindTelemetry(telemetry);

            territory.PlayerExpanded += (percent, cells, enemyCells) =>
            {
                float punch = enemyCells > 0
                    ? Mathf.Clamp(0.20f + enemyCells * 0.010f, 0.24f, 0.55f)
                    : Mathf.Clamp(0.08f + cells * 0.003f, 0.10f, 0.28f);
                avatarPresenter.Pulse(enemyCells > 0 ? 1.25f : 0.55f);
                cameraRig.Punch(punch);
                hud.NotifyExpansion(percent, cells, enemyCells);
            };

            rival.CombatResolved += (playerAdvantage, playerCount, rivalCount) =>
            {
                avatarPresenter.Pulse(playerAdvantage ? 1.35f : 1.8f);
                cameraRig.Punch(playerAdvantage ? 0.42f : 0.68f);
            };
        }
    }
}
