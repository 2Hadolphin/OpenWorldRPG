using Sirenix.OdinInspector;
using UnityEngine;
using System;
using System.IO;
using Return;


namespace Return.Database
{
    [Serializable]
    public abstract class DataEntity : CloneableScriptableObject
    {
        [HideInInspector]
        [SerializeField]
        string m_Title;
        public virtual string Title { get => m_Title; set => m_Title = value; }

        [HideInInspector]
        string m_Description;
        public virtual string Description { get => m_Description; set => m_Description = value; }

#if UNITY_EDITOR

        [PropertySpace(10, 5)]
        [PropertyOrder(-49)]
        [HorizontalGroup("Intro/Hor/Preview",Width =0.2f, PaddingLeft = 0.1f, PaddingRight = 0.1f)]
        [VerticalGroup("Intro/Hor/Preview/Toolbar")]
        [ShowInInspector]
        [PreviewField(Height =65,Alignment =ObjectFieldAlignment.Center)]
        [ReadOnly]
        [HideLabel]
        protected virtual UnityEngine.Object Preview { get => this; }

        [PropertySpace(1, 10)]
        [HorizontalGroup("Intro/Hor/Preview/Toolbar/Operate")]
        [Button(nameof(Duplicate))]
        void Copy()
        {
            Duplicate();
        }

        protected ScriptableObject Duplicate()
        {
            var copy = UnityJsonClone(HideFlags.None);
            var path = UnityEditor.AssetDatabase.GetAssetPath(this);
            //var split = path.LastIndexOf('/');
            //path = path.Substring(0,split);
            //path = m_Path.Combine(path, this.name + ".asset");
            path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(path);
            UnityEditor.AssetDatabase.CreateAsset(copy, path);
            UnityEditor.AssetDatabase.Refresh();
            UnityEditor.EditorGUIUtility.PingObject(copy);

            return copy;
        }

        /// <summary>
        /// Invoke drawing title and description.
        /// </summary>
        [PropertySpace(7,7)]
        [BoxGroup("Intro",ShowLabel =false)]
        [HorizontalGroup("Intro/Hor",Width =0.8f/*,MarginLeft =0,PaddingLeft =0*/)]
        [PropertyOrder(-50)]
        [OnInspectorGUI]
        protected virtual void DataEntityField()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Title",GUILayout.Width(80));
            TitleField();
            GUILayout.EndHorizontal();
            GUILayout.Label("Dscription", GUILayout.Width(80));
            DescriptionField();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw title field.
        /// </summary>
        protected virtual void TitleField()
        {
            var newTitle = GUILayout.TextField(Title);
            if (m_Title != newTitle)
            {
                m_Title = newTitle;
                Dirty();
            }
        }

        /// <summary>
        /// Draw description field.
        /// </summary>
        protected virtual void DescriptionField()
        {
            var newDes = GUILayout.TextArea(m_Description, GUILayout.Height(50));

            if(newDes!=m_Description)
            {
                m_Description = newDes;
                Dirty();
            }
        }

        void Reset()
        {
            Debug.LogFormat("On {0} reset.", this);
            //m_Title = $"UNASSIGNED.{System.DateTime.Now.TimeOfDay.TotalMilliseconds}";
            //m_Description = "";
        }

        /// <summary>
        /// Editor only
        /// </summary>
        public void Dirty()
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }

#endif
    }
}