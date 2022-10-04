using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Assertions;
using Return.Framework.Grids;
using Sirenix.OdinInspector;


namespace Return.Inventory
{
    [HideLabel]
    [Serializable]
    public struct InveonoryIcon 
    {
        [SerializeField]
        Sprite m_icon;

        [SerializeField]
        Volume m_volume;

        public Sprite Sprite { get => m_icon; set => m_icon = value; }
        public Volume Volume { get => m_volume; set => m_volume = value; }


        /// <summary>
        /// Get icon direction via dragging_volume.
        /// </summary>
        /// <param name="voulme">Volume of this icon instance.</param>
        public Grid2DDirection GetDirection(Volume voulme)
        {
            Assert.IsTrue(Volume.capacity==voulme.capacity,$"Require voule : {Volume} Input dragging_volume : {voulme}");

            if (voulme.width == Volume.width)
                return Grid2DDirection.Horizontal;
            else
                return Grid2DDirection.Vertical;
        }

        /// <summary>
        /// Get all indexes from pointer offset_index and icon direction.
        /// </summary>
        /// <param name="pos">Pointer offset_index.</param>
        /// <param name="dir">Sprite direction.</param>
        [Obsolete]
        public List<Vector2Int> GetVolumeIndexes(Vector2Int pos, Grid2DDirection dir)
        {
            var list = new List<Vector2Int>(Volume.capacity);

            switch (dir)
            {
                case Grid2DDirection.Horizontal:
                    for (int x = 0; x < Volume.width; x++)
                        for (int y = 0; y < Volume.height; y++)
                            list.Add(pos + new Vector2Int(x, y));
                    break;

                case Grid2DDirection.Vertical:
                    for (int x = 0; x < Volume.height; x++)
                        for (int y = 0; y < Volume.width; y++)
                            list.Add(pos + new Vector2Int(x, y));
                    break;
            }

            return list;
        }
    }



 


}