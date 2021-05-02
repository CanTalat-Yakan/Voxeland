using System.Diagnostics;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;

public class BuildScript
{
    [MenuItem("Build/Open Folder", false, 10)]
    public static void OpenFolder()
    {
        Process.Start(@"Builds");
    }

    [MenuItem("Build/Build All")]
    public static void BuildAll()
    {
        BuildWindowsServer();
        BuildLinuxServer();
        BuildWindowsClient();
        BuildWebGLClient();
    }

    #region ----- Functions
    [MenuItem("Build/Build Client (Windows)")]
    public static void BuildWindowsClient()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetBuildScenes();
        buildPlayerOptions.locationPathName = "Builds/Windows/Client/Client.exe";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows;
        buildPlayerOptions.options = BuildOptions.CompressWithLz4HC;
        BuildPlayer(buildPlayerOptions);
    }
    [MenuItem("Build/Build Client (WebGL)")]  
    public static void BuildWebGLClient()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetBuildScenes();
        buildPlayerOptions.locationPathName = "Builds/WebGL/Client";
        buildPlayerOptions.target = BuildTarget.WebGL;
        buildPlayerOptions.options = BuildOptions.CompressWithLz4HC;
        BuildPlayer(buildPlayerOptions);

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
    }
    void OnActiveBuildTargetChanged(BuildTarget _previousTarget, BuildTarget _newTarget)
    {
        if (_newTarget == BuildTarget.WebGL)
            BuildWebGLClient();
    }
    [MenuItem("Build/Build Server (Windows)")]  
    public static void BuildWindowsServer()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetBuildScenes();
        buildPlayerOptions.locationPathName = "Builds/Windows/Server/Server.exe";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows;
        buildPlayerOptions.options = BuildOptions.CompressWithLz4HC | BuildOptions.EnableHeadlessMode;
        BuildPlayer(buildPlayerOptions, "Server");
    }
    [MenuItem("Build/Build Server (Linux)")]
    public static void BuildLinuxServer()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetBuildScenes();
        buildPlayerOptions.locationPathName = "Builds/Linux/Server/Server.x86_64";
        buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
        buildPlayerOptions.options = BuildOptions.CompressWithLz4HC | BuildOptions.EnableHeadlessMode;
        BuildPlayer(buildPlayerOptions, "Server");
    }
    #endregion


    #region ----- Utilities
    public static string[] GetBuildScenes()
    {
        List<string> scenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            if (scene.enabled)
                scenes.Add(scene.path);

        return scenes.ToArray();
    }
    public static void BuildPlayer(BuildPlayerOptions _bpo, string _buildType = "Client")
    {
        BuildReport report = BuildPipeline.BuildPlayer(_bpo);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
            UnityEngine.Debug.Log($"{_bpo.target.ToString()} -{_buildType}build succeeded: {printMB(summary.totalSize)} in {summary.totalTime.TotalSeconds.ToString("0")} Seconds");

        if (summary.result == BuildResult.Failed)
            UnityEngine.Debug.Log($"Build failed with {summary.totalWarnings} Warnings!");
    }
    public static string printMB(ulong sizekB)
    {
        double sizeMB = (double)sizekB / 1048576;
        return sizeMB.ToString("0.000") + " MB";
    }
    #endregion
}