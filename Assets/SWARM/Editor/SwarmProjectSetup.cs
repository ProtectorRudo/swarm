using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Swarm.Editor
{
    [InitializeOnLoad]
    public static class SwarmProjectSetup
    {
        public const string ScenePath = "Assets/SWARM/Scenes/SwarmToy.unity";
        public const string SettingsFolder = "Assets/SWARM/Settings";
        public const string PipelinePath = SettingsFolder + "/SWARM_URP.asset";
        public const string RendererPath = SettingsFolder + "/SWARM_2DRenderer.asset";
        private const string BuiltinRendererTempPath = "Assets/UniversalRenderer.asset";

        static SwarmProjectSetup()
        {
            EditorApplication.delayCall += EnsureConfigured;
        }

        [MenuItem("SWARM/Setup Project")]
        public static void EnsureConfigured()
        {
            EnsureScene();
            EnsureBuildSettings();
            EnsureUrp2D();
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

        private static void EnsureUrp2D()
        {
            if (!AssetDatabase.IsValidFolder(SettingsFolder))
                AssetDatabase.CreateFolder("Assets/SWARM", "Settings");

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create();
                AssetDatabase.CreateAsset(pipeline, PipelinePath);

                if (AssetDatabase.LoadAssetAtPath<Object>(BuiltinRendererTempPath) != null)
                    AssetDatabase.DeleteAsset(BuiltinRendererTempPath);
                if (AssetDatabase.LoadAssetAtPath<Object>(RendererPath) != null)
                    AssetDatabase.DeleteAsset(RendererPath);

                pipeline.LoadBuiltinRendererData(RendererType._2DRenderer);
                string moveError = AssetDatabase.MoveAsset(BuiltinRendererTempPath, RendererPath);
                if (!string.IsNullOrEmpty(moveError))
                    throw new System.InvalidOperationException("SWARM URP 2D renderer move failed: " + moveError);

                EditorUtility.SetDirty(pipeline);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
        }

        private static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "ProtectorRudo";
            PlayerSettings.productName = "SWARM";
            PlayerSettings.colorSpace = ColorSpace.Linear;
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
