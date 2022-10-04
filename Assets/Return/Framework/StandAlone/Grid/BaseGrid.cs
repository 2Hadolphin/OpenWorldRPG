using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;
using System.Collections;

//using Sirenix.OdinInspector;
#if UNITY_EDITOR
#endif

namespace Return.Framework.Grids
{
    public abstract class BaseGrid<T> : IEnumerable<T>
    {
        #region Grid
#if UNITY_EDITOR
        /// <summary>
        /// Draw grid in inspector
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        protected abstract T EditorDrawContent(Rect rect,T content);

        [TableMatrix(DrawElementMethod = nameof(EditorDrawContent),SquareCells =true)]
        [ShowInInspector]
#endif
        public T[,] Contents { get; protected set; }

        public T this[int x,int y]
        {
            get
            {
                Assert.IsTrue(ValidIndex(x,y));
                return Contents[x, y];
            }

            set
            {
                Assert.IsTrue(ValidIndex(x,y));
                Contents[x, y] = value;
            }
        }

        public T this[Vector2Int index]
        {
            get
            {
                Assert.IsTrue(ValidIndex(index));
                return Contents[index.x,index.y];
            }

            set
            {
                Assert.IsTrue(ValidIndex(index));
                Contents[index.x, index.y] = value;
            }
        }

        /// <summary>
        /// Number of horizontal columns.
        /// </summary>
        public int lines;

        /// <summary>
        /// Number of vertical columns.
        /// </summary>
        public int rows;

        public float m_cellSize;

        /// <summary>
        /// Size to cacluate cell bounds.
        /// </summary>
        public float cellSize
        {
            get => m_cellSize;
            set
            {
                Assert.IsTrue(value > 0);
                m_cellSize = value;
            }
        }

        public delegate T CreateSlot(int x, int y);

        public CreateSlot CreateSlotElement;

        public virtual void Build(int row, int line)
        {
            this.lines = line;
            this.rows = row;

            Contents = new T[line, row];

            var createSlot = CreateSlotElement != null;

            for (int x = 0; x < Contents.GetLength(0); x++)
            {
                for (int y = 0; y < Contents.GetLength(1); y++)
                {
                    if (createSlot)
                        Contents[x, y] =CreateSlotElement(x, y);

                    //contents[width, height] = creation(width, height);
                    //CreateUISlot(width + "," + height, null, GetWorldPos(width, height) + new Vector2(cellSize, cellSize) * 0.5f, 15, Color.white);

                    //Debug.DrawLine(GetWorldPos(x, y), GetWorldPos(x, y + 1), Color.white, 100);
                    //Debug.DrawLine(GetWorldPos(x, y), GetWorldPos(x + 1, y), Color.white, 100);
                }
            }

            //Debug.DrawLine(GetWorldPos(0, line), GetWorldPos(row, line), Color.white, 100);
            //Debug.DrawLine(GetWorldPos(row, 0), GetWorldPos(row, line), Color.white, 100);
        }

        #endregion


        #region Valid

        #region Index
        /// <summary>
        /// Check index in grid
        /// </summary>
        public virtual bool ValidIndex(int x, int y)
        {
            return x >= 0 && x < lines && y >= 0 && y < rows;
        }


        /// <summary>
        /// Check index in grid
        /// </summary>
        public virtual bool ValidIndex(Vector2Int index)
        {
            return index.x >= 0 && index.x < lines && index.y >= 0 && index.y < rows;
        }

        #endregion


        #region Content


        /// <summary>
        /// Check slots of grid is empty or enable to use.
        /// </summary>
        public abstract bool ValidSlot(T contentSlot);

        /// <summary>
        /// Check slot enable is batch mode to <see cref="ValidSlot"/>.
        /// </summary>
        public virtual bool ValidSlots(IEnumerable<Vector2Int> points)
        {
            foreach (var point in points)
            {
                if (!ValidIndex(point))
                    return false;

                var slot = Contents[point.x, point.y];


                if (!ValidSlot(slot))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get volume indexes if exists content. 
        /// </summary>
        /// <param name="index">Index to find content.</param>
        /// <param name="slots">All indexes of content.</param>
        /// <returns>Return true if exist index</returns>
        public virtual bool GetContentIndexes(Vector2Int index, out List<Vector2Int> slots)
        {
            try
            {
                Assert.IsTrue(ValidIndex(index));

                // check empty
                if (ValidSlot(this[index]))
                {
                    slots = null;
                    return false;
                }

                var valid=GetVolume(index, out var offset, out var volume);

                Debug.Log($"Pointer volume offset : {offset}");

                Assert.IsTrue(valid);

                slots = new(volume.capacity);

                foreach (var slot in volume)
                {
                    Debug.Log($"Add volume Index {index - offset + slot} origin {index} volume {slot} offset {offset}.");
                    slots.Add(index - offset + slot);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                slots = null;
                return false;
            }
        }


        #endregion

        #region Volume


        /// <summary>
        /// Valid prefabSlot content and get pointer offset and volume. 
        /// </summary>
        /// <param name="indexes">Pointer index.</param>
        /// <param name="offset">Offset pointer index from left-buttom.</param>
        /// <param name="volume">Volume of content(cross slots).</param>
        /// <returns>Return true if content is valid.</returns>
        public virtual bool GetVolume(Vector2Int indexes, out Vector2Int offset, out Volume volume)
        {
            offset = default;
            volume = default;

            // valid pointer
            var valid = ValidIndex(indexes);
            if (!valid)
                return false;

            var content = this[indexes];

            //  valid content
            if (ValidSlot(content))
                return false;

            volume = Vector2Int.one;

            Vector2Int point;

            // right
            point = indexes;
            point.x++;
            while (ValidIndex(point))
            {
                if (!content.Equals(this[point]))
                    break;

                volume.width++;
                point.x++;

                //Debug.Log($"Volume append right {volume.width}.");
            }


            // left
            point = indexes;
            point.x--;
            while (ValidIndex(point))
            {
                if (!content.Equals(this[point]))
                    break;

                volume.width++;
                offset.x++;
                point.x--;

                Debug.Log($"Offset append left {offset.x}.");
            }


            // up
            point = indexes;
            point.y++;
            while (ValidIndex(point))
            {
                if (!content.Equals(this[point]))
                    break;

                volume.height++;
                point.y++;

                Debug.Log($"Offset append up {offset.y}.");
            }

            // down
            point = indexes;
            point.y--;
            while (ValidIndex(point))
            {
                if (!content.Equals(this[point]))
                    break;

                volume.height++;
                point.y--;
                offset.y++;
                //Debug.Log($"Volume append down {volume.height}.");
            }

            Debug.Log($"The occupied volume of {content} is \n{volume}.");

            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (Contents == null)
                yield break;

            for (int x = 0; x < lines; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    yield return Contents[x, y];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        #endregion



        #endregion


    }
}
