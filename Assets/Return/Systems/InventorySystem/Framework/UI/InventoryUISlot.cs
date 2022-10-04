using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Return.Framework.Grids;
using System;
#if UNITY_EDITOR
#endif

namespace Return.Inventory
{
    /// <summary>
    /// Cache storage content info and ui element
    /// </summary>
    public class InventoryUISlot : NullCheck,IDisposable
    {
        /// <summary>
        /// Identity of content.
        /// </summary>
        public InveonoryIcon IconData;

        /// <summary>
        /// Transform to handle sprite.
        /// </summary>
        public RectTransform IconHandler;

        /// <summary>
        /// Sprite sprite setter.
        /// </summary>
        public Image Icon;

        public Color Color;

        /// <summary>
        /// Release icon texture.
        /// </summary>
        public virtual void Dispose()
        {
            if (Icon.mainTexture.IsInstance())
                Icon.mainTexture.Destroy();
        }

        /// <summary>
        /// Get volume indexes position from ui cast.(unsafe **scale)
        /// </summary>
        public virtual IEnumerable<Vector2> GetWorldPositions()
        {

            Vector2 pos = IconHandler.position;

            var rect= IconHandler.rect;

     


            Debug.Log($"RectPosition {IconHandler.rect.position} Anchor {IconHandler.anchoredPosition}.");

            var dir = IconHandler.localEulerAngles.z == 0 ? Grid2DDirection.Horizontal : Grid2DDirection.Vertical;


            if (dir == Grid2DDirection.Vertical)
            {
                pos.x -= rect.height * 0.5f;
                pos.y -= rect.width * 0.5f;
            }
            else
            {
                pos.x -= rect.width * 0.5f;
                pos.y -= rect.height * 0.5f;
            }

            var volume = IconData.Volume;
            
            var size = IconHandler.rect.size;
            float width, height;

            width = size.x;
            height = size.y;

            width /= volume.width;
            height  /= volume.height;

            if (dir == Grid2DDirection.Vertical)
                volume = volume.Invert();

            Debug.LogError($"Get UI volume with {volume} size : {size}.");


            foreach (var index in volume)
            {
                var position = pos;

                var offsetX= (index.x + 0.5f) * width;
                var offsetY = (index.y + 0.5f) * height;

                position.x += offsetX;
                position.y += offsetY;

                Debug.Log($"Cast index {index} at {position} offset : [{offsetX},{offsetY}].");

                yield return position;
            }

            yield break;
        }

   
    }



 


}