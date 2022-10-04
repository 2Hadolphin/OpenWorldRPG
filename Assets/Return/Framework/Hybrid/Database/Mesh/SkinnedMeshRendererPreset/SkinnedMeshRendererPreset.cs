using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Return.Database;
using UnityEngine.Rendering;
//using TNet;
using UnityEngine.AddressableAssets;
using System.Linq;
using System.Threading.Tasks;
using System;
//using UnityEditor.AddressableAssets;

namespace Return
{
    public class SkinnedMeshRendererPreset : PresetDatabase
    {
        

        //[SerializeField]
        //public PresetReference<Mesh> Mesh;

        [SerializeField]
        AssetReferenceMesh m_Mesh;

        #region Skinned Mesh Renderer Data

        [SerializeField]
        SkinnedRendererBoneMap Skeleton;

        public AssetReference[] Materials;


        public ShadowCastingMode CastShadow;

#if UNITY_EDITOR

        [Button]
        public void LoadData(params SkinnedMeshRenderer[] renderers)
        {
            var length = renderers.Length;

            if (length == 0)
                return;

            var renderer = renderers[0];
            SetData(this, renderer);



            EditorHelper.Rename(this, renderer.name);
            
            if (Skeleton.IsNull())
                Skeleton = ScriptableObject.CreateInstance<SkinnedRendererBoneMap>();

            var names = renderer.bones.Select(x => x.name).ToArray();

            Skeleton.BoneNames = names;

            if (!UnityEditor.AssetDatabase.IsMainAsset(Skeleton))
                EditorHelper.WriteFile(this, Skeleton, renderer.GetRoot().name);

            Skeleton.Dirty();

            for (int i = 1; i < length; i++)
            {
                var preset = Duplicate() as SkinnedMeshRendererPreset;
                SetData(preset, renderers[i]);
            }



        }
        static void SetData(SkinnedMeshRendererPreset preset,SkinnedMeshRenderer renderer)
        {

            EditorHelper.Rename(preset, renderer.name);


            var mesh = renderer.sharedMesh;
            preset.m_Mesh = new AssetReferenceMesh(EditorUtilityTools.GetGUID(mesh)); //new AddressableReference<Mesh>(renderer.sharedMesh);

            if (UnityEditor.AssetDatabase.IsSubAsset(mesh))
                preset.m_Mesh.SetEditorSubObject(mesh);

            preset.CastShadow = renderer.shadowCastingMode;

            preset.Materials = renderer.sharedMaterials.Select(x =>
            {
                var reference = new AssetReference(EditorUtilityTools.GetGUID(x));
                return reference;
            }).ToArray();
            
            preset.Dirty();
            preset.Log();
        }

#endif

        [Button]
        void Log()
        {
            if (m_Mesh != null && m_Mesh.LoadNativeObject<Mesh>(out var handle))
                handle.Completed += (x) => LoadSkinnedRenderer(x.Result);
        }

        void LoadSkinnedRenderer(Mesh mesh)
        {
            Debug.Log("Skinned load : "+mesh);

        }

        [Button]
        public virtual SkinnedMeshRenderer SetRenderer(Transform skeletonRoot,GameObject obj=null)
        {
            //Debug.Log("Start "+UnityEditor.EditorApplication.timeSinceStartup);

            Mesh mesh;

            if (m_Mesh.IsValid()&&m_Mesh.IsDone)
                m_Mesh.Asset.Parse(out mesh);
            else
                mesh = m_Mesh.LoadAssetAsync().WaitForCompletion();

            var mats = Materials.Select(asset =>
            {
                Material mat;

                if(!asset.TryCache(out mat))
                    mat = asset.LoadAssetAsync<Material>().WaitForCompletion();
      
                //Addressables.Release(mat);
                return mat;
            }).ToArray();

            //Debug.Log(mesh);

            //Debug.Log("Stop " + UnityEditor.EditorApplication.timeSinceStartup);

            if (obj.IsNull())
            {
                obj = new GameObject(mesh.name);
                obj.transform.SetParent(skeletonRoot.parent);
            }

            var renderer = obj.InstanceIfNull<SkinnedMeshRenderer>();

            var dic = skeletonRoot.Traverse().ToDictionary(x => x.name);


            renderer.bones = Skeleton.BoneNames.Select
                (x =>
                {
                    if (dic.TryGetValue(x, out var bone))
                        return bone;
                    else
                        return null;
                }).ToArray();

            renderer.rootBone = skeletonRoot;
            renderer.shadowCastingMode = CastShadow;

            renderer.sharedMesh = mesh;

            if (mats.Length > 0)
            {
                renderer.sharedMaterials = mats;
                renderer.sharedMaterial = mats[0];
            }


            return renderer;
        }


        void SetMesh()
        {

        }


        #endregion
    }
}
