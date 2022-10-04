using Sirenix.OdinInspector.Editor;
using UnityEngine;
using System;
using System.Reflection;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using UnityEditor;
using Sirenix.OdinInspector.Editor.Resolvers;
using Sirenix.OdinInspector.Editor.ActionResolvers;

[DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
public class DropZoneAttributeDrawer : OdinAttributeDrawer<DropZoneAttribute,UnityEngine.Object>
{
    //ActionResolver Execute;


    protected override void Initialize()
    {
        //var method = this.Attribute.Method;
        //if(!string.IsNullOrEmpty(method))
        //    Execute = ActionResolver.Get(this.Property, method);
    }

    public override bool CanDrawTypeFilter(Type type)
    {
        return base.CanDrawTypeFilter(type);
    }

    GUIStyle style;

    protected override void DrawPropertyLayout(GUIContent label)
    {
        //this.CallNextDrawer(label);

        //DragAndDropUtilities.DropZone(rect,null,Attribute.Type);

        if (style == null)
        {
            style = new GUIStyle("CurveEditorBackground");
            style.normal.textColor = Attribute.TextColor;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = Attribute.FontSize;
            style.fixedHeight = Attribute.Height;
        }

        EditorGUILayout.LabelField(
            Attribute.Text,
            style,
            GUILayout.ExpandWidth(true),
            GUILayout.Height(Attribute.Height));

        //var value = DragAndDropUtilities.DropZone(Property.LastDrawnValueRect, null, Attribute.Type);
        //var value = DragAndDropUtilities.DropZone<Transform>(Property.LastDrawnValueRect, null, true);

        var drawTarget = Attribute.AlwaysDrawZone ? null : ValueEntry.SmartValue;

        var rect = Property.LastDrawnValueRect;
        rect.height = Attribute.Height;

        var value = DragAndDropUtilities.DropZone(
            rect, 
            drawTarget, 
            ValueEntry.TypeOfValue, 
            Attribute.AllowSceneObject
            );

        


        if (value != null)
        {
            //Debug.Log(this+" : "+value);
            if(value is UnityEngine.Object obj)
            {
                ValueEntry.SmartValue = obj;
                ValueEntry.ApplyChanges();
                ValueEntry.Update();
                //Execute.DoActionForAllSelectionIndices();
            }

            //string error;
            //MemberInfo memberInfo = value.GetType()
            //    .FindMember()
            //    .IsNamed(Attribute.Method)
            //    .HasNoParameters()
            //    .HasReturnType(Property.ValueEntry.BaseValueType, true)
            //    .IsInstance()
            //    .GetMember(out error);

            //if (error != null)
            //{
            //    Debug.LogError(error);
            //}
            //else
            //{
            //    Property.ValueEntry.WeakSmartValue = memberInfo.GetMemberValue(value);
            //}
        }
    }
}