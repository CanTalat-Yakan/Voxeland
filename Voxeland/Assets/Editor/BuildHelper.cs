using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

public class BuildScript
{
    [MenuItem("Build/Open Folder", false, 0)]
    public static void OpenFolder()
    {
        Process.Start(@"Builds");
    }

    [MenuItem("Build/Build Clients Only", false, 101)]
    public static void BuildClientsOnly()
    {
        BuildWindowsClient();
        BuildWebGLClient();
    }
    [MenuItem("Build/Build Server Only", false, 102)]
    public static void BuildServerOnly()
    {
        BuildWindowsServer();
        BuildLinuxServer();
    }
    [MenuItem("Build/Build Windows Only", false, 102)]
    public static void BuildWindowsOnly()
    {
        BuildWindowsServer();
        BuildWindowsClient();
    }
    [MenuItem("Build/Build All", false, 200)]
    public static void BuildAll()
    {
        BuildWindowsServer();
        BuildLinuxServer();
        BuildWindowsClient();
        BuildWebGLClient();
    }


    [MenuItem("Build/Client (WebGL)", false, 20)]
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
    [MenuItem("Build/Client (Windows)", false, 21)]
    public static void BuildWindowsClient()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetBuildScenes();
        buildPlayerOptions.locationPathName = "Builds/Windows/Client/Client.exe";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows;
        buildPlayerOptions.options = BuildOptions.CompressWithLz4HC;
        BuildPlayer(buildPlayerOptions);
    }
    [MenuItem("Build/Server (Windows)", false, 50)]
    public static void BuildWindowsServer()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetBuildScenes();
        buildPlayerOptions.locationPathName = "Builds/Windows/Server/Server.exe";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows;
        buildPlayerOptions.options = BuildOptions.CompressWithLz4HC | BuildOptions.EnableHeadlessMode;
        BuildPlayer(buildPlayerOptions, "Server");
    }
    [MenuItem("Build/Server (Linux)", false, 51)]
    public static void BuildLinuxServer()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetBuildScenes();
        buildPlayerOptions.locationPathName = "Builds/Linux/Server/Server.x86_64";
        buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
        buildPlayerOptions.options = BuildOptions.CompressWithLz4HC | BuildOptions.EnableHeadlessMode;
        BuildPlayer(buildPlayerOptions, "Server");
    }


    #region ----- Helper
    static string[] GetBuildScenes()
    {
        List<string> scenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            if (scene.enabled)
                scenes.Add(scene.path);

        return scenes.ToArray();
    }
    static void BuildPlayer(BuildPlayerOptions _bpo, string _buildType = "Client")
    {
        SetVersion();
        
        BuildReport report = BuildPipeline.BuildPlayer(_bpo);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
            UnityEngine.Debug.Log($"{_bpo.target.ToString()} - {_buildType}build succeeded: {PrintMB(summary.totalSize)} in {summary.totalTime.TotalSeconds.ToString("0")} Seconds");

        if (summary.result == BuildResult.Failed)
            UnityEngine.Debug.Log($"Build failed with {summary.totalWarnings} Warnings!");
    }
    static string PrintMB(ulong sizekB)
    {
        double sizeMB = (double)sizekB / 1048576;
        return sizeMB.ToString("0.000") + " MB";
    }
    static void SetVersion()
    {
        string[] vn = Application.version.Split('.');
        PlayerSettings.bundleVersion = $"{vn[0]}.{vn[1]}.{(int.Parse(vn[2]) + 1).ToString()}";
    }
    #endregion
}