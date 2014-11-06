
using System.IO;

using UnityEngine;
using UnityEditor;

public class BuildPackage 
{
	public static string[] m_PackageInputPaths = 
	{
		"Assets/Plugins"
	};
	public const string m_PackageOutputPath = "../build/webp.unitypackage";

	[MenuItem("Build/WebP/Build Package")]
	public static void BuildUnityPackage()
	{
		string lIOSPluginsPath = "Assets/Plugins/iOS/WebP";

		Directory.CreateDirectory(lIOSPluginsPath);

		/*
		File.Copy("../snappy/snappy.cc",                Path.Combine(lIOSPluginsPath, "snappy.cpp"), true);
		File.Copy("../snappy/snappy.h",                 Path.Combine(lIOSPluginsPath, "snappy.h"), true);
		File.Copy("../snappy/snappy-c.cc",              Path.Combine(lIOSPluginsPath, "snappy-c.cpp"), true);
		File.Copy("../snappy/snappy-c.h",               Path.Combine(lIOSPluginsPath, "snappy-c.h"), true);
		File.Copy("../snappy/snappy-internal.h",        Path.Combine(lIOSPluginsPath, "snappy-internal.h"), true);
		File.Copy("../snappy/snappy-sinksource.cc",     Path.Combine(lIOSPluginsPath, "snappy-sinksource.cpp"), true);
		File.Copy("../snappy/snappy-sinksource.h",      Path.Combine(lIOSPluginsPath, "snappy-sinksource.h"), true);
		File.Copy("../snappy/snappy-stubs-internal.cc", Path.Combine(lIOSPluginsPath, "snappy-stubs-internal.cpp"), true);
		File.Copy("../snappy/snappy-stubs-internal.h",  Path.Combine(lIOSPluginsPath, "snappy-stubs-internal.h"), true);
		File.Copy("../snappy/snappy-stubs-public.h",    Path.Combine(lIOSPluginsPath, "snappy-stubs-public.h"), true);
		*/

		AssetDatabase.Refresh();

		AssetDatabase.ExportPackage(m_PackageInputPaths, m_PackageOutputPath, ExportPackageOptions.Recurse);

		Directory.Delete(lIOSPluginsPath, true);

		AssetDatabase.Refresh();
	}
}
