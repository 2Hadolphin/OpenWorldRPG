using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using System.Text;
using System;

[Serializable]
public class AvatarSelector:SerializedScriptableObject
{
    public const string Panel_Preview = "Vertical";
    public const string Panel_Avatar = "Vertical/Avatar";
    public const string Panel_Button = "Vertical/Avatar/Button";

    [HideInInspector]
    [HideLabel]
    [ReadOnly]
    public AvatarMaskBodyPart PartDof { get; protected set; } = AvatarMaskBodyPart.Body;

    [VerticalGroup(Panel_Avatar)]
    [Tooltip("Character Texture")]
    public Texture2D AvatarTexture;

    [HideLabel]
    [ReadOnly]
    [ShowInInspector]
    [VerticalGroup(Panel_Avatar)]
    string m_Part;





    [HideLabel]
    [PropertyOrder(-4)]
    [HorizontalGroup(Panel_Preview)]
    [OnInspectorGUI]
    private void mDrawPreview()
    {
        mRect.width = EditorGUIUtility.currentViewWidth - 50;
        DrawPreview(mRect);
    }

    Rect mRect=new Rect(0, EditorGUIUtility.singleLineHeight,150,300);
    
    public void DrawPreview(Rect rect)
    {
        if (!AvatarTexture)
        {
            EditorGUILayout.HelpBox(string.Format("Missing texure resource : {0}", AvatarTexture), MessageType.Error);
            return;
        }



        var width = rect.width;

        var ratio = (float)AvatarTexture.height / AvatarTexture.width;

        var targetWidth = Mathf.Clamp(width * 0.35f, 150, 200);

        var targetHeight = targetWidth * ratio;

        var size = new Vector2(targetWidth, targetHeight);
        mRect.size = size;

        //EditorGUILayout.BeginVertical();
        //EditorGUILayout.Space(rect.rows, false);

        //EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.Space(rect.lines, false);

        var curRect=EditorGUILayout.BeginVertical(GUILayout.MaxWidth(targetWidth),GUILayout.MaxHeight(targetHeight));
        GUILayout.Label(AvatarTexture, GUI.skin.FindStyle("flow node 0"), GUILayout.Width(size.x), GUILayout.Height(size.y));
        EditorGUILayout.EndVertical();

        //EditorGUILayout.EndHorizontal();
        //EditorGUILayout.EndVertical();

        ratio = curRect.width / 100f;

        var length = ButtonPositions.Count;
        var color = GUI.color;
        GUI.color = new Color(1, 1, 1, 0.8f);
        var style = new GUIStyle() { alignment=TextAnchor.MiddleCenter };
        
        style.normal.background = null;


        for (int i = 0; i < length; i++)
        {
            var mRect = new Rect(ButtonPositions[i] * ratio + curRect.position, ButtonSize * ratio);
            if (GUI.Button(mRect, i == sn ? PointTexture_Selected : PointTexture, style))
            {
                sn = i;
                m_Part = PartDof.ToString();
                PartDof = (AvatarMaskBodyPart)(i+1);
                //switch (i)
                //{
                //    case 6:
                //        PartDof = HumanPartDof.LeftThumb;
                //        m_Part = "LeftHand";
                //        break;
                //    case 7:
                //        PartDof = HumanPartDof.RightThumb;
                //        m_Part = "RightHand";
                //        break;

                //    default:
                //        PartDof = (HumanPartDof)i;
                //        m_Part = PartDof.ToString();
                //        break;
                //}
            }
        }

        GUI.color = color;

    }

    #region PartSelectButton
    [VerticalGroup(Panel_Button)]
    [Tooltip("Button Icon")]
    public Texture PointTexture;
    [VerticalGroup(Panel_Button)]
    [Tooltip("High Light Button Icon")]
    public Texture PointTexture_Selected;
    [VerticalGroup(Panel_Button)]
    [Tooltip("Size of button when lines of avatar rect is 100")]
    public Vector2 ButtonSize=new Vector2(17, 17);
    [PropertyOrder(2)]
    [VerticalGroup(Panel_Button)]
    [Tooltip("Copy button data as const string.")]
    [Button("Copy")]
    void CopyButton()
    {
        var sb = new StringBuilder();
        var ratio = mRect.width / 200;
        foreach (var p in ButtonPositions)
        {
            sb.AppendLine(string.Format("new Vector2({0}f,{1}f),", (p.x) / ratio, (p.y) / ratio));
        }
        EditorGUIUtility.systemCopyBuffer = sb.ToString();
    }

    [PropertyOrder(3)]
    [VerticalGroup(Panel_Button)]
    [ShowInInspector]
    public List<Vector2> ButtonPositions = new List<Vector2>
    {
new Vector2(41.98f,75.9f),
new Vector2(41.3f,20f),
new Vector2(54.4f,144f),
new Vector2(28.9f,124.9f),
new Vector2(65.3f,42.8f),
new Vector2(18.4f,46f),
new Vector2(71.1f,90.6f),
new Vector2(12.6f,90.7f),

    };

    [VerticalGroup(Panel_Button)]
    [LabelText("Selected Button Number")]
    [ShowInInspector]
    [ReadOnly]
    public int sn { get; protected set; } = 0;


    #endregion
}
