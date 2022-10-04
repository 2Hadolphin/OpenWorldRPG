using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using Sirenix.OdinInspector;


//using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Return.Framework.Grids
{



    [Serializable]
    public abstract class Grid3D<T> : BaseGrid<T>
    {
        public Grid3D(int width, int height, float cellSize, Vector3 originPos)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.worldPos = originPos;

            Contents = new T[width, height];
            textArray = new TextMesh[width, height];

            for (int x = 0; x < Contents.GetLength(0); x++)
            {
                for (int y = 0; y < Contents.GetLength(1); y++)
                {
                    textArray[x, y] = CreateWorldText(x + "," + y, null, GetWorldPos(x, y) + new Vector3(cellSize, 0, cellSize) * 0.5f, 15, Color.white);

                    Debug.DrawLine(GetWorldPos(x, y), GetWorldPos(x, y + 1), Color.white, 100);
                    Debug.DrawLine(GetWorldPos(x, y), GetWorldPos(x + 1, y), Color.white, 100);
                }
            }

            Debug.DrawLine(GetWorldPos(0, height), GetWorldPos(width, height), Color.white, 100);
            Debug.DrawLine(GetWorldPos(width, 0), GetWorldPos(width, height), Color.white, 100);

        }

        public Vector3 worldPos;

        public int width;
        public int height;

        public TextMesh[,] textArray;

        public void SetValue(Vector3 pos, T value)
        {
            GetCoordinate(pos, out var x, out var y);
            SetValue(x, y, value);
        }

        public void GetValue(Vector3 pos, out string value)
        {
            GetCoordinate(pos, out var x, out var y);
            value = textArray[x, y].text;
        }

        public virtual void GetValue(Vector3 pos, out T value)
        {
            GetCoordinate(pos, out var x, out var y);
            value = Contents[x, y];
        }


        public void GetPosition(Vector3 pos, out Vector3 middle)
        {
            int x, y;

            if (GetCoordinate(pos, out x, out y))
                middle = new Vector3(x + 0.5f, 0, y + 0.5f) * cellSize + worldPos;
            else
                middle = default;
        }

        bool GetCoordinate(Vector3 pos, out int x, out int y)
        {
            x = Mathf.FloorToInt((pos - worldPos).x / cellSize);
            y = Mathf.FloorToInt((pos - worldPos).z / cellSize);
            Debug.Log(pos + " ** " + x + " " + y);

            return x >= 0 && x < width && y >= 0 && y < height;
        }

        void SetValue(int x, int y, T value)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                Contents[x, y] = value;
                textArray[x, y].text = Contents[x, y].ToString();
                Debug.Log(x + " " + y);
            }
            else
                Debug.LogError(string.Format(" Out of range lines {0} -- {1} rows {2}--{3}", width, x, height, y));
        }

        Vector3 GetWorldPos(int x, int y)
        {
            return new Vector3(x, 0, y) * cellSize + worldPos;
        }

        public static TextMesh CreateWorldText(string text, Transform parent = null, Vector3 localPos = default(Vector3), int fontSize = 40, Color color = default(Color), TextAlignment textAlignment = TextAlignment.Center, TextAnchor textAnchor = TextAnchor.MiddleCenter, int shortingOrder = 0)
        {
            if (color == null)
                color = Color.white;

            return CreateWorldText(parent, text, localPos, fontSize, (Color)color, textAnchor, textAlignment, shortingOrder);
        }

        public static TextMesh CreateWorldText(Transform parent = null, string text = null, Vector3 localPos = default(Vector3), int fontSize = 40, Color color = default(Color), TextAnchor textAnchor = TextAnchor.MiddleCenter, TextAlignment textAlignment = TextAlignment.Center, int shortingOrder = 0)
        {
            var go = new GameObject("World_Text", typeof(TextMesh))
            {
                hideFlags = HideFlags.DontSave
            };

            SceneVisibilityManager.instance.DisablePicking(go, true);

            //GameObject.Destroy(go, 30);

            var tf = go.transform;
            tf.SetParent(parent, false);
            tf.localPosition = localPos;
            tf.localRotation = Quaternion.Euler(90, 0, 0);

            var textMesh = tf.GetComponent<TextMesh>();
            textMesh.anchor = textAnchor;
            textMesh.alignment = textAlignment;
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.color = color;

            textMesh.GetComponent<MeshRenderer>().sortingOrder = shortingOrder;
            return textMesh;
        }


        public int GetRotation(GridContentDirection dir)
        {
            return dir switch
            {
                GridContentDirection.Up => 0,
                GridContentDirection.Right => 90,
                GridContentDirection.Down => 180,
                GridContentDirection.Left => 270,
                _ => throw new KeyNotFoundException($"{dir} is invalid direction in inventory."),
            };
        }

        public List<Vector2Int> GetVolumeIndexes(Vector2Int pos, GridContentDirection dir)
        {
            var list = new List<Vector2Int>(width * height);

            switch (dir)
            {
                case GridContentDirection.Up:
                    for (int x = 0; x < width; x++)
                        for (int y = 0; y < height; y++)
                            list.Add(pos + new Vector2Int(x, y));
                    break;

                case GridContentDirection.Right:
                    for (int x = 0; x < height; x++)
                        for (int y = 0; y < width; y++)
                            list.Add(pos + new Vector2Int(x, y));
                    break;

                case GridContentDirection.Down:
                    break;
                case GridContentDirection.Left:
                    break;
            }

            return list;
        }
    }

    //[Flags]
    public enum GridContentDirection
    {
        Up,
        Right,
        Down,
        Left,
        //All = Up | Right | Down | Left
    }
}
