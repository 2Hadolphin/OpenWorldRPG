using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Return.Framework.Grids;

namespace Return
{
    [ExecuteInEditMode]
    public class TestGrid : MonoBehaviour
    {
        [Range(1,999)]
        public int width=10;
        [Range(1, 999)]
        public int height=10;

        [Range(0.5f, 100)]
        public float CellSize = 10f;

        [SerializeField]
        Grid3D<int> Grid;



        [SerializeField]
        bool autoGeuerate;

        private void Start()
        {
            if(autoGeuerate)
                GenerateGrid();
        }

        private void Update()
        {
            if (Grid == null)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                Grid.GetValue(GetMouseWorldPosition(), out int value);

                if (value > 0)
                    return;

                Grid.GetPosition(GetMouseWorldPosition(), out var pos);
                Instantiate(House, pos, Quaternion.identity);

                Grid.SetValue(GetMouseWorldPosition(), 1);
            }

            if (Input.GetMouseButtonDown(1))
                Grid.SetValue(GetMouseWorldPosition(), 0);
        }

        public GameObject House;

        [Button]
        void GenerateGrid()
        {
            //Grid = new Grid3D<int>(width, height, CellSize,transform.position);

        }

        public Vector3 GetMouseWorldPosition()
        {  
            var cam=UnityEditor.EditorApplication.isPlaying?Camera.main: UnityEditor.SceneView.lastActiveSceneView.camera;

            var mouse = Input.mousePosition;
            mouse.z = Vector3.Project(gameObject.transform.position-cam.transform.position,gameObject.transform.up).magnitude;

            var pos = GetMouseWorldPositionWithY(mouse, cam);
            Debug.Log(pos);


            return pos;
        }

        private static Vector3 GetMouseWorldPositionWithY(Vector3 mousePosition, Camera camera)
        {
            Debug.Log("input " + mousePosition);
            var worldPos = camera.ScreenToWorldPoint(mousePosition);
            return worldPos;
        }
    }
}
