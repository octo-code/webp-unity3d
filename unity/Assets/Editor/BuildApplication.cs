
using System.IO;

using UnityEngine;
using UnityEditor;

public class BuildApplication 
{
	public const string m_PackageOutputPath = @"../build/webp.unitypackage";

    private static string GetProjectName()
    {
        string[] s = Application.dataPath.Split('/');
        return s[s.Length - 2];
    }
 
    private static string[] GetScenePaths()
    {
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
 
        for(int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }
 
        return scenes;
    }

	[MenuItem("Build/WebP/Build Application")]
	public static void BuildUnityApplication()
	{
        BuildTarget lTarget = EditorUserBuildSettings.activeBuildTarget;

        BuildOptions lOptions = BuildOptions.None;

        lOptions |= BuildOptions.Development;

        string lOutput = "untitled";

        switch (lTarget)
        {
            case BuildTarget.iPhone:
            {
                lOutput = "build/iOS";
                break;
            }
        }

        BuildPipeline.BuildPlayer(GetScenePaths(), "build/iOS", lTarget, lOptions);
	}
}
