using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Return
{
    public class Layers
    {
        public static readonly LayerMask AllLayers = -1;

        public static readonly int Default = LayerMask.NameToLayer(nameof(Default));

        public static LayerMask without(params int[] exclude)
        {
            return AllLayers.Exclude(exclude);
        }


        #region Camera
        public static readonly int FirstPerson_Body = LayerMask.NameToLayer(nameof(FirstPerson_Body));
        public static readonly int FirstPerson_Handle = LayerMask.NameToLayer(nameof(FirstPerson_Handle));
        #endregion

        #region Physics
        public static readonly int Physics_Normal = LayerMask.NameToLayer(nameof(Physics_Normal));
        public static readonly int Physics_Detail = LayerMask.NameToLayer(nameof(Physics_Detail));
        public static readonly int Physics_Nav = LayerMask.NameToLayer(nameof(Physics_Nav));
        public static readonly int Physics_Dynamic = LayerMask.NameToLayer(nameof(Physics_Dynamic));
        public static readonly int Physics_Vehicle = LayerMask.NameToLayer(nameof(Physics_Vehicle));
        public static readonly int Physics_Interactable = LayerMask.NameToLayer(nameof(Physics_Interactable));
        #endregion

        #region Graphic
        public static readonly int PostProcessing = LayerMask.NameToLayer(nameof(PostProcessing));
        public static readonly int Plants = LayerMask.NameToLayer(nameof(Plants));
        public static readonly int Partical = LayerMask.NameToLayer(nameof(Partical));


        /// <summary>
        /// None render, shadow only, used to first person character. 
        /// </summary>
        public static readonly int Shadow = LayerMask.NameToLayer(nameof(Shadow));
        #endregion

        #region Logic
        public static readonly int Ignore = LayerMask.NameToLayer(nameof(Ignore));
        #endregion

    }
}
