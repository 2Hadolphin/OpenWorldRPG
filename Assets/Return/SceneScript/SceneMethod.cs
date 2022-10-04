using UnityEngine;

namespace Return.SceneModule 
{
    public abstract class SceneModule : MonoBehaviour
    {
        protected static SCMA Manager;
        protected static SCMAData data;

        public abstract void Init();
    }
}


