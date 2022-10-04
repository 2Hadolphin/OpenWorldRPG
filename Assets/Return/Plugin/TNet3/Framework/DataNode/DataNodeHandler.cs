using UnityEngine;
using TNet;
using Sirenix.OdinInspector;
using System.IO;
using System;

namespace Return
{
    [HideLabel]
    [Serializable]
    public struct DataNodeHandler
    {
        //bool customPath => ArchiveOption == ArchiveOption.Custom;

        //[ShowIf(nameof(customPath)]
        //string CustomDataPath;

        //[HideInInspector]
        [FolderPath(AbsolutePath =true,RequireExistingPath =false)]
        [SerializeField]
        string m_dataPath;

        [SerializeField]
        string m_fileName;

        [SerializeField]
        ArchiveOption m_archiveOption;

        /// <summary>
        /// Application.persistentDataPath..DataPath
        /// </summary>
        public string DataPath { get => m_dataPath; set => m_dataPath = value; }
        public string FileName { get => m_fileName; set => m_fileName = value; }
        public ArchiveOption ArchiveOption { get => m_archiveOption; set => m_archiveOption = value; }

        public bool TryLoadData(out DataNode dataNode)
        {
            var folderPath = GetFolderPath();

            Debug.Log(folderPath);

            var dataPath = GetFilePath();

            if (File.Exists(dataPath))
            {
                dataNode = DataNode.Read(dataPath, true);
                return true;
            }
            else
            {
                dataNode = null;
                return false;
            }

        }

        public void SaveData(DataNode dataNode)
        {
            var path = GetFilePath();
            dataNode.Write(path, DataNode.SaveType.Text);
        }

        [Button]
        void ShowData()
        {
            var folderPath = GetFolderPath();
                //Path.Combine(Application.persistentDataPath, DataPath);

            if (Directory.Exists(folderPath))
                System.Diagnostics.Process.Start(folderPath);
            else
                Debug.LogWarning($"None exist file at {GetFilePath()}.");
        }

        [Button]
        void CleanData()
        {
            var dataPath = GetFilePath();
            if (File.Exists(dataPath))
                File.Delete(dataPath);
        }

        public string GetFolderPath()
        {
            return ArchiveOption switch
            {
                ArchiveOption.Persistent => Path.Combine(Application.persistentDataPath, DataPath),
                ArchiveOption.Custom => DataPath,
                _ => throw new NotImplementedException(ArchiveOption.ToString()),
            };
        }

        public string GetFilePath()
        {
            return ArchiveOption switch
            {
                ArchiveOption.Persistent => Path.Combine(Application.persistentDataPath, DataPath, FileName +(Path.HasExtension(FileName)?null : ".txt")),
                ArchiveOption.Custom => Path.Combine(DataPath, FileName + (Path.HasExtension(FileName) ? null : ".txt")),
                _ => throw new NotImplementedException(ArchiveOption.ToString()),
            };
        }


#if Unity_Editor




#endif
    }


    public enum ArchiveOption
    {
        Persistent,
        Custom,
    }

}