using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

public class MyBuildPostprocessor
{
	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
	{
        if (target == BuildTarget.MetroPlayer)
        {
            //OnPostprocessBuildWSA(pathToBuiltProject);
        }
        else if (target == BuildTarget.iPhone)
        {
            OnPostprocessBuildIOS(pathToBuiltProject);
        }
	}

	private static void OnPostprocessBuildIOS(string pathToBuiltProject)
	{
        Directory.CreateDirectory(Path.Combine(pathToBuiltProject, "Libraries/snappy"));
        File.Copy("../snappy/snappy.cc",                Path.Combine(pathToBuiltProject, "Libraries/snappy/snappy.cc"), true);
        File.Copy("../snappy/snappy.h",                 Path.Combine(pathToBuiltProject, "Libraries/snappy/snappy.h"), true);
        File.Copy("../snappy/snappy-c.cc",              Path.Combine(pathToBuiltProject, "Libraries/snappy/snappy-c.cc"), true);
        File.Copy("../snappy/snappy-c.h",               Path.Combine(pathToBuiltProject, "Libraries/snappy/snappy-c.h"), true);
        File.Copy("../snappy/snappy-internal.h",        Path.Combine(pathToBuiltProject, "Libraries/snappy/snappy-internal.h"), true);
        File.Copy("../snappy/snappy-sinksource.cc",     Path.Combine(pathToBuiltProject, "Libraries/snappy/snappy-sinksource.cc"), true);
        File.Copy("../snappy/snappy-sinksource.h",      Path.Combine(pathToBuiltProject, "Libraries/snappy/snappy-sinksource.h"), true);
        File.Copy("../snappy/snappy-stubs-internal.cc", Path.Combine(pathToBuiltProject, "Libraries/snappy/snappy-internal.cc"), true);
        File.Copy("../snappy/snappy-stubs-internal.h",  Path.Combine(pathToBuiltProject, "Libraries/snappy/snappy-internal.h"), true);
        File.Copy("../snappy/snappy-stubs-public.h",    Path.Combine(pathToBuiltProject, "Libraries/snappy/snappy-stubs-public.h"), true);

		// TODO: for now project api is not exposed, so you need to add files manually
	}

	private static void OnPostprocessBuildWSA(string pathToBuiltProject)
	{
		string exportedPath = Path.Combine(pathToBuiltProject, PlayerSettings.productName);

		string[] filesToSearch = new[] { "App.cpp", "App.xaml.cpp", "App.cs", "App.xaml.cs" };

		bool patched = false;
		for (int i = 0; i < filesToSearch.Length; i++)
		{
			string path = Path.Combine(exportedPath, filesToSearch[i]);
			if (path.Contains(".cpp") && PatchFile(path, "m_AppCallbacks->SetBridge(_bridge);", "m_AppCallbacks->SetBridge(_bridge);\r\n\tm_AppCallbacks->LoadGfxNativePlugin(\"RenderingPlugin.dll\");"))
			{
				patched = true;
				break;
			}
			if (path.Contains(".cs") && PatchFile(path, "appCallbacks.SetBridge(_bridge);", "appCallbacks.SetBridge(_bridge);\r\n\t\t\t\tappCallbacks.LoadGfxNativePlugin(\"RenderingPlugin.dll\");"))
			{
				patched = true;
				break;
			}
		}

		if (!patched) Debug.LogError("Failed to patch file");
	}


	private static bool PatchFile(string fileName, string targetString, string replacement)
	{
		if (File.Exists(fileName) == false) return false;

		string text = File.ReadAllText(fileName);

		if (text.IndexOf(targetString) == -1) return false;

		// Already patched ?
		if (text.IndexOf(replacement) != -1) return true;

		text = text.Replace(targetString, replacement);

		File.WriteAllText(fileName, text);

		return true;
	}
}
