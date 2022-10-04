using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
public class ExportBundle : MonoBehaviour
{
    [MenuItem("Tools/Build Asset Bundles")]
    static void BuildAssetBundles()
    {

        BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.StandaloneWindows);
    }
}