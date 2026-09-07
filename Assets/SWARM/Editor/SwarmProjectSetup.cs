using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Swarm.Editor
{
    [InitializeOnLoad]
    public static class SwarmProjectSetup
    {
        public const string ScenePath = "Assets/SWARM/Scenes/SwarmToy.unity";

        static SwarmProjectSetup()
        {
            EditorApplication.delayCall += EnsureConfigured;
        }

        [MenuItem("SWARM/Setup Project")]
        public static void EnsureConfigured()
        {
            EnsureScene();
            EnsureBuildSettings();
            ConfigurePlayer();
        }

        private static void EnsureScene()
        {
            if (File.Exists(ScenePath)) return;

            if (!AssetDatabase.IsValidFolder("Assets/SWARM/Scenes"))
                AssetDatabase.CreateFolder("Assets/SWARM", "Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes.Length == 1 && scenes[0].path == ScenePath && scenes[0].enabled) return;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        private static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "ProtectorRudo";
            PlayerSettings.productName = "SWARM";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.protectorrudo.swarm");
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            EditorUserBuildSettings.buildAppBundle = false;
        }
    }
}
