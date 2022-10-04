using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Return.Editors
{
    public class EditorGameObjectReviewer : OdinEditorWindow
    {
        [MenuItem("Tools/PrefabViewer")]
        static void CreateWizard()
        {
            var window = EditorWindow.CreateWindow<EditorGameObjectReviewer>();
        }


        [ListDrawerSettings(Expanded =true,ShowIndexLabels = false,NumberOfItemsPerPage =7,CustomAddFunction =nameof(Add))]
        [SerializeField]
        List<GameObject> m_Objects;

        public virtual void Add(object obj)
        {
            if(obj is GameObject go)
            {
                go.SetActive(false);
                Objects.Add(go);
            }

        }

        public List<GameObject> Objects { get => m_Objects; set => m_Objects = value; }

        [OnValueChanged(nameof(ShowObject))]
        [SerializeField]
        int Index;


        [HorizontalGroup("Control")]
        [Button]
        public virtual void LastObject()
        {
            Index = Objects.Loop(--Index);
            ShowObject();
        }

        [HorizontalGroup("Control")]
        [Button]
        public virtual void NextObject()
        {
            Index = Objects.Loop(++Index);
            ShowObject();
        }

        public virtual void ShowObject()
        {
           var target =Objects[Index];

            if(target==null)
            {
                Objects.RemoveAt(Index);
                ShowObject();
                return;
            }

            foreach (var go in Objects)
            {
                var activate = go == target;
                go.SetActive(activate);
            }
        }
    }
}
