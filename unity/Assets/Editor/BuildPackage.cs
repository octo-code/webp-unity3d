
using System.IO;

using UnityEngine;
using UnityEditor;

public class BuildPackage 
{
	public static string[] m_PackageInputPaths = 
	{
		"Assets/Plugins"
	};
	public const string m_PackageOutputPath = @"../build/webp.unitypackage";

	[MenuItem("Build/WebP/Build Package")]
	public static void BuildUnityPackage()
	{
		var lIOSPluginsPath = @"Assets/Plugins/iOS/WebP";

		Directory.CreateDirectory(lIOSPluginsPath);

        var lSourceDirectory = new DirectoryInfo("../webp/libwebp/src");

        var lSrcFiles       = lSourceDirectory.GetFiles("*.c", SearchOption.AllDirectories);
        var lHeaderFiles    = lSourceDirectory.GetFiles("*.h", SearchOption.AllDirectories);

        for (int lIndex = 0; lIndex < lSrcFiles.Length; ++lIndex)
        {
            string lRelativePath = lSrcFiles[lIndex].FullName.Substring(lSourceDirectory.FullName.Length + 1);
            Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(lIOSPluginsPath, lRelativePath)));
            File.Copy(lSrcFiles[lIndex].FullName, Path.Combine(lIOSPluginsPath, lRelativePath), true);
        }

        for (int lIndex = 0; lIndex < lHeaderFiles.Length; ++lIndex)
        {
            string lRelativePath = lHeaderFiles[lIndex].FullName.Substring(lSourceDirectory.FullName.Length + 1);
            Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(lIOSPluginsPath, lRelativePath)));
            File.Copy(lHeaderFiles[lIndex].FullName, Path.Combine(lIOSPluginsPath, lRelativePath), true);
        }

        AssetDatabase.Refresh();

		AssetDatabase.ExportPackage(m_PackageInputPaths, m_PackageOutputPath, ExportPackageOptions.Recurse);

		Directory.Delete(lIOSPluginsPath, true);

		AssetDatabase.Refresh();
	}
}
