using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Return.Animations;

namespace Return.Items
{
    public abstract class PostureAdjustHandle: PresetDatabase
    {
        #region Runtime
        public abstract IAnimationStreamHandler CreateAdjustJob(PR offset,Transform handle,AbstractItem item, Animator animator);
        #endregion




#if UNITY_EDITOR

        /// <summary>
        /// Editor Draw Wizer
        /// </summary>
        /// <param name="postureData"></param>
        public abstract void DrawPostureWizer(Return.Editors.ItemPostureAdjustWizer wizer, HandleConfig config,Transform handle,string handleName);


#endif
    }
}

