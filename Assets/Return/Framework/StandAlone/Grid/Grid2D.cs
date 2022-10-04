using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

//using Sirenix.OdinInspector;
#if UNITY_EDITOR
#endif

namespace Return.Framework.Grids
{
    public abstract class Grid2D<T> : BaseGrid<T>
    {

        /// <summary>
        /// World coordinate.
        /// </summary>
        public Func<Vector2> worldPos;

        //public Func<Vector2> spacing;

        public Func<RectOffset> padding;

        #region Valid

        /// <summary>
        /// Check contents is departable.
        /// </summary>
        public virtual bool ValidPack(IEnumerable<Vector2Int> indexes)
        {
            {
                // search volume content then valid indexes contains
                //var hash = indexes.ToHashSet();

                //foreach (var index in indexes)
                //{

                //}
            }

            // search volume border and valid overlap content

            var count = indexes.Count();

            // down
            Dictionary<int, int> x_max=new(count);
            // down
            Dictionary<int, int> x_min = new(count);

            // down
            Dictionary<int, int> y_max = new(count);
            // down
            Dictionary<int, int> y_min = new(count);

            // cache edge
            foreach (var index in indexes)
            {
                if (x_max.ContainsKey(index.y))
                {
                    if (x_max[index.y] < index.x)
                        x_max[index.y] = index.x;
                }
                else
                    x_max.Add(index.y, index.x);

                if (x_min.ContainsKey(index.y))
                {
                    if (x_min[index.y] > index.x)
                        x_min[index.y] = index.x;
                }
                else
                    x_min.Add(index.y, index.x);

                if (y_max.ContainsKey(index.x))
                {
                    if (y_max[index.x] < index.y)
                        y_max[index.x] = index.y;
                }
                else
                    y_max.Add(index.x, index.y);

                if (y_min.ContainsKey(index.x))
                {
                    if (y_min[index.x] > index.y)
                        y_min[index.x] = index.y;
                }
                else
                    y_min.Add(index.x, index.y);
            }

            bool valid(Vector2Int nextIndex,T content)
            {
                if (ValidIndex(nextIndex))
                {
                    var next = this[nextIndex];
                    if (!next.Equals(default(T)) && !next.Equals(content))
                        return false;
                }

                return true;
            }

            foreach (var right in x_max)
            {
                var index = new Vector2Int(right.Value, right.Key);
                var content = this[index];
                if (content == null)
                    continue;

                var nextIndex = index;
                nextIndex.x++;

                if (!valid(nextIndex, content))
                    return false;
            }

            foreach (var left in x_min)
            {
                var index = new Vector2Int(left.Value, left.Key);
                var content = this[index];
                if (content == null)
                    continue;

                var nextIndex = index;
                nextIndex.x--;

                if (!valid(nextIndex, content))
                    return false;
            }

            foreach (var up in y_max)
            {
                var index = new Vector2Int(up.Key, up.Value);
                var content = this[index];
                if (content == null)
                    continue;

                var nextIndex = index;
                nextIndex.y++;

                if (!valid(nextIndex, content))
                    return false;
            }

            foreach (var down in y_min)
            {
                var index = new Vector2Int(down.Key, down.Value);
                var content = this[index];
                if (content == null)
                    continue;

                var nextIndex = index;
                nextIndex.y--;

                if (!valid(nextIndex, content))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check prefabSlot with volume at position.
        /// </summary>   
        [Obsolete]
        public bool ValidSlots(Vector2Int indexes, Volume volume, out IEnumerable<Vector2Int> slots, bool ignoreEdge = false)
        {
            slots = null;
            Queue<Vector2Int> points = new(volume.width * volume.height);

            // loop volume contents
            for (int volumeX = 0; volumeX < volume.width; volumeX++)
            {
                for (int volumeY = 0; volumeY < volume.height; volumeY++)
                {
                    var x = volumeX + indexes.x;
                    var y = volumeY + indexes.y;

                    if (ValidIndex(x, y))
                    {
                        var content = Contents[x, y];

                        if (content != null)
                            return false;

                        points.Enqueue(new Vector2Int(x, y));
                    }
                    else
                    {
                        Debug.LogError($" Out of range lines {lines} -- {x} rows {rows}--{y}");
                        return false;
                    }
                }
            }

            slots = points;
            return true;
        }

        [Obsolete]
        public virtual T[] Find(int id, int row = -1, int line = -1)
        {
            if (row < 0)
                row = 0;

            if (line < 0)
                line = 0;

            for (int x = 0; x < Contents.GetLength(0); x++)
            {
                for (int y = 0; y < Contents.GetLength(1); y++)
                {


                }
            }


            return null;
        }



        protected virtual bool SearchVertical(int x, int y, int nums, Func<T, bool> valid, out int breakIndex)
        {
            for (int i = y; i < y + nums; i++)
            {
                if (!ValidIndex(x, i))
                {
                    breakIndex = i;
                    return false;
                }

                if (!valid(Contents[x, i]))
                {
                    breakIndex = i;
                    return false;
                }
            }

            breakIndex = default;
            return true;
        }

        protected virtual bool SearchHorizontal(int x, int y, int nums, Func<T, bool> valid, out int breakIndex)
        {
            for (int i = x; i < x + nums; i++)
            {
                if (!ValidIndex(i, x))
                {
                    breakIndex = i;
                    return false;
                }


                if (!valid(Contents[i, x]))
                {
                    breakIndex = i;
                    return false;
                }
            }

            breakIndex = default;
            return true;
        }


        //public bool ValidSlots(IEnumerable<Vector2Int> list)
        //{
        //    foreach (var index in list)
        //    {
        //        if (!ValidIndex(index) || !ValidSlot(this[index]))
        //            return false;
        //    }

        //    return true;
        //}

        [Obsolete]
        public virtual bool GetFreeSlots(Volume volume, List<Vector2Int> list)
        {
            list = new(volume.capacity);




            return false;

            Vector2Int pos = Vector2Int.zero;

            bool SearchOriging()
            {
                var slot = pos;
                return ValidIndex(slot) && ValidSlot(Contents[slot.x, slot.y]);
            }

            bool SearchTopRight()
            {
                var slot = volume.TopRight(pos);
                return ValidIndex(slot) && ValidSlot(Contents[slot.x, slot.y]);
            }

            bool SearchTopLeft()
            {
                var slot = volume.TopLeft(pos);
                return ValidIndex(slot) && ValidSlot(Contents[slot.x, slot.y]);
            }

            bool SearchDownRight()
            {
                var slot = volume.DownRight(pos);
                return ValidIndex(slot) && ValidSlot(Contents[slot.x, slot.y]);
            }

    

            for (int x = 0; x < lines; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    if (SearchHorizontal(x, y, volume.width, ValidSlot, out var breakIndex))
                    {
                        if (SearchVertical(x, y, volume.height, ValidSlot, out breakIndex))
                        {

                        }

                    }
                    else
                    {
                        x = breakIndex + 1;
                        break;
                    }

                }
            }

            return list.Count == volume.capacity;
        }

        public bool SearchSpaceTopLeft(Volume volume, out IEnumerable<Vector2Int> indexes)
        {
            for (int y = lines-volume.height; y >=0; y--)
            {
                for (int x = 0; x < Contents.GetLength(0); x++)
                {
                    indexes = volume.GetIndexes(new Vector2Int(x, y));

                    Debug.Log("Valid slot : \n" + new Vector2Int(x, y));

                    if (ValidSlots(indexes))
                    {

                        Debug.Log("Found empty slots : \n" + string.Join("\n", indexes));
                        return true;
                    }
                }
            }

            indexes = null;
            return false;
        }


        public bool SearchSpace(Volume volume, out IEnumerable<Vector2Int> indexes)
        {
            for (int y = 0; y < Contents.GetLength(1); y++)
            {
                for (int x = 0; x < Contents.GetLength(0); x++)
                {
                    indexes = volume.GetIndexes(new Vector2Int(x, y));

                    if (ValidSlots(indexes))
                    {
                        //Debug.Log("Found empty slots : \n" + string.Join("\n", indexes));
                        return true;
                    }
                }
            }

            indexes = null;
            return false;
        }


        #endregion


        #region Content

        /// <summary>
        /// Get value via pointer
        /// </summary>
        /// <param name="pos">Pointer position.</param>
        public T GetValue(Vector2 pos)
        {
            if (GetIndex(pos, out var x, out var y))
                return Contents[x, y];
            else
                return default;
        }

        /// <summary>
        /// Get value via pointer
        /// </summary>
        /// <param name="pos">Pointer position.</param>
        public T GetValue(Vector2Int pos)
        {
            Assert.IsTrue(ValidIndex(pos));
            return Contents[pos.x, pos.y];
        }

        /// <summary>
        /// Set value via pointer
        /// </summary>
        /// <param name="pos">Pointer position.</param>
        public void SetValue(Vector2 pos, T value)
        {
            GetIndex(pos, out var x, out var y);
            SetValue(x, y, value);
        }

        public void SetValue(Vector2Int index,T value)
        {
            //Debug.Log($"Set [{index.x},{index.y}] from {this[index]} to {value}.");
            this[index] = value;
        }


        /// <summary>
        /// Set value via Index
        /// </summary>
        public void SetValue(int x, int y, T value)
        {
            Assert.IsTrue(x >= 0 && x < lines && y >= 0 && y < rows, $" Out of range lines {lines} -- {x} rows {rows}--{y}");

            Contents[x, y] = value;
            //Debug.Log($"Set [{x},{y}] with {value}.");
        }


        public virtual void SetVolumeValue(Vector2Int min, Vector2Int max,T content)
        {
            for (int x = min.x; x < max.x+1; x++)
            {
                for (int y = min.y; y < max.y+1; y++)
                {
                    //Debug.Log($"Set [{x},{y}] : {content}.");
                    SetValue(x,y,content);
                }
            }
        }

        #endregion



        #region Convert

        /// <summary>
        /// ???
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="middle"></param>
        public void GetPosition(Vector2 pos, out Vector2 middle)
        {
            Vector2 offset;

            {
                var padding = this.padding?.Invoke();

                if (padding != null)
                    offset = new Vector2(padding.left, padding.top);
                else
                    offset = Vector2.zero;
            }
       

            if (GetIndex(pos, out int x, out int y))
                middle = new Vector2(x + 0.5f, y + 0.5f) * cellSize + worldPos() + offset;
            else
                middle = default;
        }

        /// <summary>
        /// Return index position.
        /// </summary>
        public void GetIndexPosition(Vector2Int index, out Vector2 position,bool middle=true)
        {
            Vector2 padding;

            {
                var offset = this.padding?.Invoke();

                if (offset != null)
                    padding = new Vector2(offset.left, offset.bottom);
                else
                    padding = Vector2.zero;
            }

            if(middle)
                position = new Vector2(index.x + 0.5f, index.y + 0.5f) * cellSize + worldPos() + padding;
            else
                position= new Vector2(index.x, index.y) * cellSize + worldPos() + padding;
        }

        /// <summary>
        /// Get selected index via pointer position.
        /// </summary>
        /// <param name="pos">Pointer position.</param>
        /// <param name="x">Index-width</param>
        /// <param name="y">Index-height</param>
        /// <returns></returns>
        public bool GetIndex(Vector2 pos, out int x, out int y)
        {
            x = Mathf.FloorToInt((pos - worldPos()).x / cellSize);
            y = Mathf.FloorToInt((pos - worldPos()).y / cellSize);
            Debug.Log(pos + " ** " + x + " " + y);

            return x >= 0 && x < lines && y >= 0 && y < rows;
        }

        /// <summary>
        /// Get grid index from rect position(left-down).
        /// </summary>
        public bool GetIndex(Vector2 pos, out Vector2Int indexes)
        {
            var offset = worldPos();

            {
                var padding = this.padding?.Invoke();

                if (padding != null)
                    offset += new Vector2(padding.left, padding.top);
            }

            Debug.Log($"Grids padding : {offset}");

            indexes = new(
                    Mathf.FloorToInt((pos - offset).x / cellSize),
                    Mathf.FloorToInt((pos - offset).y / cellSize)
                    );

            var valid= indexes.x >= 0 && indexes.x < lines && indexes.y >= 0 && indexes.y < rows;

            Debug.Log($"Get {(valid ?null:"invalid")} index [{indexes.x},{indexes.y}] at {pos-offset} with size : {cellSize}.");

            return valid;
        }


        public void GetMinMax(IEnumerable<Vector2Int> indexes, out Vector2Int min, out Vector2Int max)
        {
            min = new Vector2Int(lines, rows);
            max = Vector2Int.zero;

            foreach (var index in indexes)
            {
                if (min.x > index.x)
                    min.x = index.x;

                if (min.y > index.y)
                    min.y = index.y;

                if (max.x < index.x)
                    max.x = index.x;

                if (max.y < index.y)
                    max.y = index.y;
            }
        }

        /// <summary>
        /// Get top down point with content volume. 
        /// </summary>
        public virtual Vector2Int GetPointerOffset(int x, int y)
        {
            var indexes=new Vector2Int(x, y);

            var content = Contents[x, y];

            if (content == null)
                return default;

            while (x > 0)
            {
                if (!content.Equals(Contents[x-1, y]))
                    break;

                x--;
            }

            while (y > 0)
            {
                if (!content.Equals(Contents[x, y-1]))
                    break;

                y--;
            }

            return new Vector2Int(x,y) - indexes;
        }

        #endregion
    }

    public enum Grid2DDirection
    {
        Horizontal,
        Vertical
    }

}
