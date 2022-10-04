using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Return
{
    /// <summary>
    /// Const class to manage materials
    /// </summary>
    public class Materials : ScriptableObject,IDisposable
    {
        HashSet<Material> Mats;

        public static Material Lit
        {
            get => new(Shader.Find("Universal Render Pipeline/Lit"));
        }

        public static Material Unlit
        {
            get => new(Shader.Find("Universal Render Pipeline/Unlit"));
        }

        public static bool IsManaged(Material mat)
        {
#if UNITY_EDITOR
            var path=AssetDatabase.GetAssetPath(mat);
            return string.IsNullOrEmpty(path);
#else
            
#endif
            return false;
        }

        public void Dispose()
        {
            void destroy(Material mat)
            {
#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlaying)
                    DestroyImmediate(mat, false);
                else
                    Destroy(mat);
#else
                    Destroy(mat);
#endif
            };

            foreach (var mat in Mats)
            {
                destroy(mat);
            }

        }
    }
}