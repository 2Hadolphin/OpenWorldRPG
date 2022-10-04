using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Sirenix.OdinInspector;

namespace Return.Framework.Grids
{
    /// <summary>
    /// Content size in inventory system.
    /// </summary>
    [HideLabel]
    [Serializable]
    public struct Volume : IEnumerable<Vector2Int>,IEquatable<Volume>
    {
        public Volume(int x, int y)
        {
            this.width = (byte)x;
            this.height = (byte)y;
        }

        public Volume(byte x, byte y)
        {
            this.width = x;
            this.height = y;
        }

        public byte width;
        public byte height;

    
        public Volume Invert()
        {
            return new(height, width);
        }

        /// <summary>
        /// Max size of volume.
        /// </summary>
        public int max => width > height ? width : height;

        /// <summary>
        /// Min size of volume.
        /// </summary>
        public int min => width > height ? height : width;


        public Vector2Int TopRight(Vector2Int pos)
        {
            return pos + this - Vector2Int.one;
        }

        public Vector2Int DownRight(Vector2Int pos)
        {
            pos.x += width-1;
            return pos;
        }

        public Vector2Int TopLeft(Vector2Int pos)
        {
            pos.y += height -1;
            return pos;
        }

        public static implicit operator Vector2Int(Volume volume)
        {
            return new Vector2Int(volume.width, volume.height);
        }

        public static implicit operator Volume(Vector2Int indexes)
        {
            return new Volume() 
            {
                width = (byte)indexes.x, 
                height = (byte)indexes.y 
            };
        }

        public int capacity=> width * height;

        public IEnumerable<Vector2Int> GetIndexes(Vector2Int pointer, Grid2DDirection dir = Grid2DDirection.Horizontal)
        {
            int row, line;

            if (dir == Grid2DDirection.Horizontal)
            {
                row = width;
                line = height;
            }
            else
            {
                row = height;
                line = width;
            }

            for (int x = 0; x < row; x++)
            {
                for (int y = 0; y < line; y++)
                {
                    yield return pointer + new Vector2Int(x, y);
                }
            }


        }

        public IEnumerable<Vector2Int> GetIndexes(Vector2Int pointer,Vector2Int offset,Grid2DDirection dir=Grid2DDirection.Horizontal)
        {
            int row,line;

            if(dir==Grid2DDirection.Horizontal)
            {
                row = width;
                line = height;
            }
            else
            {
                row = height;
                line = width;
            }

            for (int x = 0; x < row; x++)
            {
                for (int y = 0; y < line; y++)
                {
                    yield return pointer + offset + new Vector2Int(x, y);
                }
            }
                

            yield break;
        }

        public IEnumerator<Vector2Int> GetEnumerator()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    yield return new Vector2Int(x,y);
                }
            }

            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return 
                $"\n" +
                $"width : {width}\n" +
                $"height : {height}\n" +
                $"capacity : {capacity}";
        }


        /// <summary>
        /// Valid width and height between volumes.
        /// </summary>
        public static bool operator ==(Volume a, Volume b)
        {
            return a.width == b.width && a.height == b.height;
        }

        /// <summary>
        /// Valid width and height between volumes.
        /// </summary>
        public static bool operator !=(Volume a, Volume b)
        {
            return a.width != b.width || a.height != b.height;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(Volume other)
        {
            return other==this;
        }
    }
}