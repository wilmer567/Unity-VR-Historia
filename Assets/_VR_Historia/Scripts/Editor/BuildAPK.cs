using UnityEditor;
using UnityEditor.Build.Reporting;
using System.Linq;

public static class BuildAPK
{
    public static void Build()
    {
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/VRHistoria.apk",
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            UnityEngine.Debug.Log($"Build succeeded: {summary.totalSize} bytes, {summary.outputPath}");
            EditorApplication.Exit(0);
        }
        else
        {
            UnityEngine.Debug.LogError($"Build failed: {summary.result}");
            foreach (var step in report.steps)
            {
                foreach (var msg in step.messages)
                {
                    if (msg.type == UnityEngine.LogType.Error || msg.type == UnityEngine.LogType.Warning)
                        UnityEngine.Debug.LogError($"[{step.name}] {msg.content}");
                }
            }
            EditorApplication.Exit(1);
        }
    }
}
