using UnityEngine;


namespace Return.Scenes
{
    public class SceneReference : ScriptableObject
    {
        [SerializeField]
        SceneData m_scene;

        public SceneData Scene { get => m_scene; set => m_scene = value; }
    }

}