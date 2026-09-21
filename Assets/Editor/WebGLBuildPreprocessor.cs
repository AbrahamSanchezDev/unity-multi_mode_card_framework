using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class WebGLBuildPreprocessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    private List<string> modifiedAsmdefs = new List<string>();

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.WebGL)
        {
            UnityEngine.Debug.Log("WebGL Build: Disabling Meta XR assemblies...");
            DisableMetaXRForWebGL();
        }
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.WebGL)
        {
            UnityEngine.Debug.Log("WebGL Build: Re-enabling Meta XR assemblies...");
            ReenableMetaXRForAndroid();
        }
    }

    private void DisableMetaXRForWebGL()
    {
        string cacheDir = "Library/PackageCache";
        
        if (!Directory.Exists(cacheDir))
        {
            UnityEngine.Debug.LogWarning("Library/PackageCache not found!");
            return;
        }

        // Find all Meta XR packages in cache
        var metaPackageDirs = Directory.GetDirectories(cacheDir)
            .Where(d => Path.GetFileName(d).StartsWith("com.meta.xr"))
            .ToList();

        UnityEngine.Debug.Log($"Found {metaPackageDirs.Count} Meta XR packages");

        // Find all .asmdef files in Meta XR packages
        foreach (string packageDir in metaPackageDirs)
        {
            var asmdefFiles = Directory.GetFiles(packageDir, "*.asmdef", SearchOption.AllDirectories);
            
            foreach (string asmdefPath in asmdefFiles)
            {
                UnityEngine.Debug.Log($"Modifying: {asmdefPath}");
                ModifyAsmdefForWebGL(asmdefPath, true);
                modifiedAsmdefs.Add(asmdefPath);
            }
        }

        AssetDatabase.Refresh();
        UnityEngine.Debug.Log($"Modified {modifiedAsmdefs.Count} .asmdef files");
    }

    private void ReenableMetaXRForAndroid()
    {
        if (modifiedAsmdefs.Count == 0)
            return;

        foreach (string asmdefPath in modifiedAsmdefs)
        {
            if (File.Exists(asmdefPath))
            {
                UnityEngine.Debug.Log($"Restoring: {asmdefPath}");
                ModifyAsmdefForWebGL(asmdefPath, false);
            }
        }

        modifiedAsmdefs.Clear();
        AssetDatabase.Refresh();
    }

    private void ModifyAsmdefForWebGL(string asmdefPath, bool excludeWebGL)
    {
        if (!File.Exists(asmdefPath))
            return;

        string json = File.ReadAllText(asmdefPath);
        var asmdefData = JsonUtility.FromJson<AssemblyDefinitionData>(json);

        if (asmdefData == null)
            return;

        if (excludeWebGL)
        {
            // Add WebGL to excludePlatforms if not already there
            if (asmdefData.excludePlatforms == null)
                asmdefData.excludePlatforms = new string[0];

            if (!System.Linq.Enumerable.Contains(asmdefData.excludePlatforms, "WebGL"))
            {
                var excludeList = new List<string>(asmdefData.excludePlatforms);
                excludeList.Add("WebGL");
                asmdefData.excludePlatforms = excludeList.ToArray();
            }
        }
        else
        {
            // Remove WebGL from excludePlatforms
            if (asmdefData.excludePlatforms != null)
            {
                var excludeList = new List<string>(asmdefData.excludePlatforms);
                excludeList.Remove("WebGL");
                asmdefData.excludePlatforms = excludeList.ToArray();
            }
        }

        string modifiedJson = JsonUtility.ToJson(asmdefData, true);
        File.WriteAllText(asmdefPath, modifiedJson);
    }
}

[System.Serializable]
public class AssemblyDefinitionData
{
    public string name;
    public string[] references = new string[0];
    public string[] includePlatforms = new string[0];
    public string[] excludePlatforms = new string[0];
    public bool allowUnsafeCode;
    public bool overrideReferences;
    public string[] precompiledReferences = new string[0];
    public bool autoReferenced = true;
    public string[] defineConstraints = new string[0];
}