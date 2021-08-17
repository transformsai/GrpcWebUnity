using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

public class GrpcWebUnityBuildProcessor : ScriptableObject
{
    // Set in the script instance (select the script asset look at the inspector)
    public TextAsset Script;

    [PostProcessBuild(-1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        var sourceFileName = GetScriptPath();
        var destFile = Path.Combine($"{pathToBuiltProject}", Path.GetFileName(sourceFileName));
        File.Copy(sourceFileName, destFile, true);

    }

    public static string GetScriptPath()
    {
        
        var instance = CreateInstance<GrpcWebUnityBuildProcessor>();
        var internalPath = AssetDatabase.GetAssetPath(instance.Script);
        DestroyImmediate(instance);
        var pkgInfo = PackageInfo.FindForAssetPath(internalPath);
        var relPath = internalPath.Substring(pkgInfo.assetPath.Length + 1);
        var resolvedPath = Path.Combine(pkgInfo.resolvedPath, relPath);
        var path = Path.GetFullPath(resolvedPath);
        return path;

    }
    
}
