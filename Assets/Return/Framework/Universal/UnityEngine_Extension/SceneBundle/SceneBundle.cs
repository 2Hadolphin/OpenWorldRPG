using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Database;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;


namespace Return.Scenes
{
    public partial class SceneBundle : PresetDatabase
    {
        [SerializeField]
        List<SceneData> m_QuickScenes;
        public List<SceneData> QuickScenes { get => m_QuickScenes; set => m_QuickScenes = value; }
    }

}