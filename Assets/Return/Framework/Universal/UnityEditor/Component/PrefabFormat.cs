#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Return.Editors
{
    public class PrefabFormat
    {

        [MenuItem("GameObject/ApplyPrefabFormat", true)]
        static bool CheckGameObject()
        {
            return Selection.activeObject is GameObject;
        }

        [MenuItem("GameObject/ApplyPrefabFormat")]
        public static void ApplyAllChildFormat()
        {
            var objs = Selection.gameObjects;
            Undo.RecordObjects(objs, nameof(ApplyAllChildFormat));

            var cache = new HashSet<GameObject>(objs.Length);
            var cacheComps = new HashSet<Component>(objs.Length);

            var rules = new PrefabFormat();

            foreach (var obj in objs)
            {
                if (!cache.Add(obj))
                    continue;

                {
                    // bind renderer
                    var tfs = obj.transform.Traverse();
                    foreach (var child in tfs)
                    {
                        if (!child.name.Contains("Model", StringComparison.CurrentCultureIgnoreCase))
                            continue;
                        if (!child.TryGetComponent<Renderer>(out var renderer))
                            continue;
                        if (!cacheComps.Add(renderer))
                            continue;

                        renderer.tag = rules.Renderer.tag;
                        renderer.gameObject.layer = rules.Renderer.layer;
                    }

                    var renderers = obj.GetComponentsInChildren<Renderer>(true);
                    foreach (var renderer in renderers)
                    {
                        if (!cacheComps.Add(renderer))
                            continue;
                        renderer.tag = rules.Renderer.tag;
                        renderer.gameObject.layer = rules.Renderer.layer;
                    }
                }




                {
                    // bind collider with tag
                    var cols = obj.GetComponentsInChildren<Collider>(true);
                    var cacheCols = new HashSet<Collider>(cols.Length);

                    foreach (var col in cols)
                    {
                        if (!cacheCols.Add(col))
                            continue;

                        if (col.tag == Tags.Interactable)
                        {
                            col.gameObject.layer = Layers.Ignore;
                            continue;
                        }


                        // split physics and graphic layer
                        if (!col.TryGetComponent(out Renderer renderer))
                        {
                            col.tag = rules.Collider.tag;
                            col.gameObject.layer = rules.Collider.layer;
                            continue;
                        }

                        var go = new GameObject("Collider")
                        {
                            tag = rules.Collider.tag,
                            layer = rules.Collider.layer,
                        };


                        {
                            // copy transform
                            go.transform.parent = col.transform.parent;
                            go.transform.Copy(col.transform);
                            go.transform.SetSiblingIndex(col.transform.GetSiblingIndex());

                            // copy colliders
                            var comps = col.GetComponents<Collider>();
                            foreach (var comp in comps)
                            {
                                if (comp != col && !cacheCols.Add(comp))
                                    continue;

                                var newCol = go.AddComponent(comp.GetType());
                                EditorUtility.CopySerialized(comp, newCol);
                                GameObject.DestroyImmediate(comp);
                            }
                        }
                    }
                }


                var configs = obj.GetAllTags(Tags.Config);

                foreach (var config in configs)
                {
                    config.layer = Layers.Ignore;
                }
            }
        }

        public class Format : IComparable<Format>
        {
            public Type type = typeof(Component);
            public int layer = Layers.Default;
            public string tag = Tags.Config;

            public int Order = 0;

            public int CompareTo(Format other)
            {
                return Order - other.Order;
            }
        }


        public Format Renderer =
            new()
            {
                type=typeof(Renderer),
                layer = Layers.Default,
                tag=Tags.Renderer,
                Order=-1
            };


        public Format Collider =
            new()
            {
                type = typeof(Collider),
                layer = Layers.Physics_Dynamic,
                tag = Tags.Collider,
                Order = 1
            };


    }
}
#endif