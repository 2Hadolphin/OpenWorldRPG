#if UNITY_EDITOR
using TNet;
using UnityEngine;
using Sirenix.OdinInspector.Editor;


namespace Return
{
    sealed class DataNodeDrawer : OdinValueDrawer<DataNode>
    {


        protected override void DrawPropertyLayout(GUIContent label)
        {
            
            var node=ValueEntry.SmartValue;

            GUILayout.Label(label);

            //Property.Update();

            

            //accessibility = Property.GetValue() as GameConfig.Accessibility;
            //EditorGUI.BeginProperty(position, label, Property);
            

            //Rect foldout = new Rect(position);
            //foldout.height = EditorGUIUtility.singleLineHeight;
            //Property.isExpanded = EditorGUI.Foldout(foldout, Property.isExpanded, label, true);

            //if (Property.isExpanded)
            //{
            //    string[] lines = accessibility.data.ToString().Split('\n')[1..]; // skip the datanode name
            //    for (int i = 0; i < lines.Length; i++)
            //    {
            //        var r = new Rect(position.x, position.y + ((i + 1) * EditorGUIUtility.singleLineHeight), position.width, EditorGUIUtility.singleLineHeight);
            //        EditorGUI.LabelField(r, lines[i].Replace("\t", "  "));
            //    }

            //    EditorGUI.EndProperty();
            //    Property.serializedObject.ApplyModifiedProperties();
            //}
        }

        //GameConfig.Accessibility accessibility;

        //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        //{
        //    return (property.isExpanded
        //        ? (accessibility == null ? 2 : accessibility.data.ToString().Split('\n').Length)
        //        : 1)
        //        * EditorGUIUtility.singleLineHeight;
        //}


    }

}

#endif