using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Swarm.Editor
{
    public static class SwarmBuild
    {
        private const string OutputPath = "Builds/Android/SWARM-0.1.apk";

        [MenuItem("SWARM/Build Android APK")]
        public static void BuildAndroid()
        {
            SwarmProjectSetup.EnsureConfigured();
            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath) ?? "Builds/Android");

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                throw new InvalidOperationException("Could not switch Unity active build target to Android.");

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
        }
    }
}
