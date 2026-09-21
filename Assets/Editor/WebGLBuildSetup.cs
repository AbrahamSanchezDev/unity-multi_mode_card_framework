// using UnityEditor;
// using UnityEditor.Build;
// using UnityEditor.Build.Reporting;
// using System.IO;
// using UnityEngine;

// public class WebGLBuildSetup : IPreprocessBuildWithReport, IPostprocessBuildWithReport
// {
//     private string manifestPath = "Packages/manifest.json";
//     private string originalManifest;

//     public int callbackOrder => -1;

//     public void OnPreprocessBuild(BuildReport report)
//     {
//         if (report.summary.platform == BuildTarget.WebGL)
//         {
//             Debug.Log("WebGL build detected - removing Meta XR packages...");
//             RemoveMetaPackagesForWebGL();
//         }
//     }

//     public void OnPostprocessBuild(BuildReport report)
//     {
//         if (report.summary.platform == BuildTarget.WebGL)
//         {
//             Debug.Log("Restoring Meta XR packages...");
//             RestoreMetaPackages();
//         }
//     }

//     private void RemoveMetaPackagesForWebGL()
//     {
//         if (!File.Exists(manifestPath))
//             return;

//         originalManifest = File.ReadAllText(manifestPath);
//         string modified = originalManifest;

//         // Remove Meta XR packages
//         modified = System.Text.RegularExpressions.Regex.Replace(modified, @",?\s*""com\.meta\.xr[^""]*"":\s*""[^""]*""", "");

//         File.WriteAllText(manifestPath, modified);
//         AssetDatabase.Refresh();
//         Debug.Log("Meta packages removed from manifest");
//     }

//     private void RestoreMetaPackages()
//     {
//         if (originalManifest != null && File.Exists(manifestPath))
//         {
//             File.WriteAllText(manifestPath, originalManifest);
//             AssetDatabase.Refresh();
//             Debug.Log("Meta packages restored");
//         }
//     }
// }