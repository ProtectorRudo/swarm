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

            var player = new GameObject("AvatarRoot_PLAYER");
            player.transform.SetParent(root.transform, false);
            player.transform.position = BattlePalette.BasePosition(BattlePalette.PlayerOwner, ArenaHalfExtents);
            var motor = player.AddComponent<PlayerMotor>();
            motor.Initialize(input, ArenaHalfExtents);
            var avatarPresenter = player.AddComponent<BlobPresenter>();
            avatarPresenter.Initialize(motor);

            var swarmObject = new GameObject("Swarm_PLAYER");
            swarmObject.transform.SetParent(root.transform, false);
            var swarm = swarmObject.AddComponent<SwarmController>();
            swarm.Initialize(player.transform, motor);
            swarm.SetVisualColor(BattlePalette.Color(BattlePalette.PlayerOwner));
            swarm.SetCount(3);
            avatarPresenter.BindSwarm(swarm);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(root.transform, false);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.AddComponent<AudioListener>();
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.045f, 0.057f, 0.092f, 1f);
            var cameraRig = cameraObject.AddComponent<CameraRig>();
            cameraRig.Initialize(camera, player.transform, ArenaHalfExtents);
            cameraRig.BindSwarm(swarm);

            var territoryObject = new GameObject("Territory");
            territoryObject.transform.SetParent(root.transform, false);
            var territory = territoryObject.AddComponent<TerritorySystem>();
            territory.Initialize(ArenaHalfExtents);

            var territoryVisualObject = new GameObject("TerritoryVisual");
            territoryVisualObject.transform.SetParent(root.transform, false);
            territoryVisualObject.AddComponent<TerritoryPresenter>().Initialize(territory, ArenaHalfExtents);

            var battleObject = new GameObject("BattleArenaDirector");
            battleObject.transform.SetParent(root.transform, false);
            var battle = battleObject.AddComponent<BattleArenaDirector>();
            battle.Initialize(player.transform, motor, swarm, territory, ArenaHalfExtents);

            for (int owner = 2; owner <= BattlePalette.ParticipantCount; owner++)
            {
                var botObject = new GameObject("Bot_" + BattlePalette.Name(owner));
                botObject.transform.SetParent(root.transform, false);
                var bot = botObject.AddComponent<RivalBot>();
                bot.Initialize(owner, territory, ArenaHalfExtents, battle);
                battle.RegisterBot(bot);
            }

            var hudObject = new GameObject("HUD");
            hudObject.transform.SetParent(root.transform, false);
            var hud = hudObject.AddComponent<ToyHud>();
            hud.Initialize(input, swarm);
            hud.BindBattle(battle);

            var matchObject = new GameObject("MatchDirector");
            matchObject.transform.SetParent(root.transform, false);
            var match = matchObject.AddComponent<MatchDirector>();
            match.Initialize(input, motor, territory, swarm, hud);
            match.BindBattle(battle);

            var feedbackObject = new GameObject("FeedbackDirector");
            feedbackObject.transform.SetParent(root.transform, false);
            var feedback = feedbackObject.AddComponent<FeedbackDirector>();
            feedback.Initialize(territory, match, battle);

            var pickupsObject = new GameObject("Pickups");
            pickupsObject.transform.SetParent(root.transform, false);
            var pickups = pickupsObject.AddComponent<PickupSystem>();
            pickups.Initialize(player.transform, swarm, battle, avatarPresenter, cameraRig, hud, feedback, ArenaHalfExtents);
            match.BindPickups(pickups);

            var bots = battle.Bots;
            for (int i = 0; i < bots.Count; i++)
                if (bots[i] != null) bots[i].BindPickups(pickups);

            var telemetryObject = new GameObject("FirstTestTelemetry");
            telemetryObject.transform.SetParent(root.transform, false);
            var telemetry = telemetryObject.AddComponent<FirstTestTelemetry>();
            telemetry.Initialize(match, territory, swarm, battle);
            hud.BindTelemetry(telemetry);

            battle.CombatResolved += (winner, loser, decisive) =>
            {
                if (winner != BattlePalette.PlayerOwner && loser != BattlePalette.PlayerOwner) return;
                bool playerWon = winner == BattlePalette.PlayerOwner;
                avatarPresenter.Pulse(playerWon ? (decisive ? 1.55f : 1.0f) : 1.75f);
                cameraRig.Punch(playerWon ? 0.16f : 0.24f);
            };

            battle.ParticipantDefeated += (winner, loser) =>
            {
                if (winner == BattlePalette.PlayerOwner)
                {
                    avatarPresenter.Pulse(2.0f);
                    cameraRig.Punch(0.28f);
                }
                else if (loser == BattlePalette.PlayerOwner)
                {
                    avatarPresenter.Pulse(2.3f);
                    cameraRig.Punch(0.30f);
                }
            };

            territory.PlayerExpanded += (percent, cells, enemyCells) =>
            {
                if (enemyCells <= 0) return;
                avatarPresenter.Pulse(0.65f);
                hud.NotifyExpansion(percent, cells, enemyCells);
            };
        }
    }
}
