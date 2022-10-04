using UnityEngine;
using System;
using Sirenix.OdinInspector;
using TNet;
using System.IO;
using System.Text;

namespace Return
{
    /// <summary>
    /// Handler to save file and load file.
    /// </summary>
    public class ModularWrapperHandler:BaseComponent
    {
        #region Persistent

        /// <summary>
        /// Extract DataNode from textAsset.
        /// </summary>
        [SerializeField]
        UnityEngine.TextAsset asset;


        /// <summary>
        /// Extract DataNode from disk file.
        /// </summary>
        [IgnoredByTNet]
        [SerializeField]
        [FolderPath]
        string m_Path;

        [IgnoredByTNet]
        public string dataPath
        {
            get
            {
                return m_Path;

                // check asset from disk or native bundle
                if (asset == null)
                    return m_Path;
                else
                    return null;
            }
            set => m_Path = value;
        }

        #endregion

        [IgnoredByTNet]
        public GameObject Target;

        // runtime cache
        protected DataNode Node { get; set; }

        // get data form disk path or textAsset inside assetbundle

        #region Preset



        [IgnoredByTNet]
        static string GetFolder
            =>
#if UNITY_EDITOR
            Application.dataPath;
#else
            Application.persistentDataPath;
#endif

        [Button]
        public void Save()
        {
             if (Target == null)
                Target = gameObject;

            Node = Target.Serialize();

            if (string.IsNullOrEmpty(dataPath))
            {
#if UNITY_EDITOR
                dataPath = GetFolder + "/modularClass.bytes";
#else
                dataPath = GetFolder + "/modularClass.bytes";
#endif
            }

            Node.Write(dataPath);

#if UNITY_EDITOR
            //if(Editors.EditorAssetsUtility.)
#else
              
#endif
        }

        [Button]
        public void Load()
        {
            if (asset.NotNull())
            {
                // load from asset file
                Node = DataNode.Read(asset.bytes);
            }
            else if (!string.IsNullOrEmpty(dataPath))
            {
                // load from disk file
                Node = DataNode.Read(dataPath);
            }
            else
            {
                Debug.LogError(dataPath);
                return;
            }

            Debug.Log(Node.children);

            if (Node.CanBeInstantiated())
                Node.Instantiate(gameObject);
            else
                Debug.LogError("Failure to instantiate");
        }

        [Button]
        public void Create()
        {
            if (Node == null)
                return;

            if (Node.CanBeInstantiated())
                Node.Instantiate();


        }

        [Button]
        public void CleanNode()
        {
            Node = new DataNode();
        }

        #endregion



    }
}