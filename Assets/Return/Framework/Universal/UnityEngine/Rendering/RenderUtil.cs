
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

namespace Return
{

    /// <summary>
    /// Rendering utility tools.
    /// </summary>
    public static class RenderUtil
    {

        public static void Shot(RenderTexture renderTexture)
        {
            byte[] bytes = toTexture2D(renderTexture).EncodeToPNG();
            var path = EditorUtility.SaveFilePanel("Save", string.Empty, "Picture.png", "png");
            System.IO.File.WriteAllBytes(path, bytes);
        }


        public static Texture2D toTexture2D(RenderTexture rTex)
        {
            Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
            RenderTexture.active = rTex;
            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();
            return tex;
        }
    }
}
#endif