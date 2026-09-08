using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Swarm.Editor
{
    public static class SwarmBuild
    {
        private const string OutputPath = "Builds/Android/SWARM-0.4-battle-arena.apk";

        [MenuItem("SWARM/Build Android APK")]
        public static void BuildAndroid()
        {
            SwarmProjectSetup.EnsureConfigured();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ValidateProject();

            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath) ?? "Builds/Android");

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                if (Application.isBatchMode)
                {
                    throw new InvalidOperationException(
                        "Batch build must launch Unity with -buildTarget Android. " +
                        "SwitchActiveBuildTarget is not supported in batch mode.");
                }

                if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                    throw new InvalidOperationException("Could not switch Unity active build target to Android.");
            }

            var options = new BuildPlayerOptions
            {
                scenes = new[] { SwarmProjectSetup.ScenePath },
                locationPathName = OutputPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.Development | BuildOptions.CompressWithLz4HC
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("SWARM Android build failed: " + report.summary.result);

            Debug.Log("SWARM_APK_READY=" + Path.GetFullPath(OutputPath));
            Debug.Log("SWARM_APK_SIZE_BYTES=" + report.summary.totalSize);
            Debug.Log("SWARM_BUILD_VERSION=0.4-battle-arena");
        }

        private static void ValidateProject()
        {
            if (!File.Exists(SwarmProjectSetup.ScenePath))
                throw new InvalidOperationException("SWARM scene is missing: " + SwarmProjectSetup.ScenePath);

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(SwarmProjectSetup.PipelinePath);
            if (pipeline == null)
                throw new InvalidOperationException("SWARM URP asset is missing.");

            var renderer = AssetDatabase.LoadAssetAtPath<Renderer2DData>(SwarmProjectSetup.RendererPath);
            if (renderer == null)
                throw new InvalidOperationException("SWARM URP 2D renderer asset is missing.");

            if (GraphicsSettings.defaultRenderPipeline != pipeline && QualitySettings.renderPipeline != pipeline)
                throw new InvalidOperationException("SWARM URP asset exists but is not the active render pipeline.");

            if (PlayerSettings.colorSpace != ColorSpace.Linear)
                throw new InvalidOperationException("SWARM must build in Linear color space.");

            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.Portrait)
                throw new InvalidOperationException("SWARM must build in portrait orientation.");
        }
    }
}
