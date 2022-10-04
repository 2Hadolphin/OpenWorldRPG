using UnityEngine;
using System;
using Return.Framework.Grids;
using Random = UnityEngine.Random;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Return.Inventory
{
    [Serializable]
    public class InventoryGrid : Grid2D<InventoryUISlot>
    {

        public override bool ValidSlot(InventoryUISlot contentSlot)
        {
            return contentSlot == null;
        }

        /// <summary>
        /// Ignore edge when conflict with border.
        /// </summary>
        public bool ignoreEdge=true;




#if UNITY_EDITOR
        protected override InventoryUISlot EditorDrawContent(Rect rect,InventoryUISlot content)
        {
            var valid = content != null;

            EditorGUI.DrawRect(
                rect,
                valid?
                content.Color:
                new Color(0.5f, 0.5f, 0.5f)
                );

            //if (content != null)
            //    GUILayout.Label(content.IconData.ToString());

            return content;
        }
#endif
    }



 


}