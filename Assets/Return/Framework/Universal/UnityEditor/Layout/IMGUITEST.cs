using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Reflection;
using UnityEngine.UIElements;

public static class IMGUITEST
{
    //反射获取Toolbar类型
    static Type m_toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
    //反射获取GUIView类型
    static Type m_guiViewType = typeof(Editor).Assembly.GetType("UnityEditor.GUIView");
    static PropertyInfo m_viewVisualTree = m_guiViewType.GetProperty("visualTree",
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    static FieldInfo m_imguiContainerOnGui = typeof(IMGUIContainer).GetField("m_OnGUIHandler",
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    static ScriptableObject m_currentToolbar;

    /// <summary>
    /// Callback for toolbar OnGUI method.
    /// </summary>
    public static Action OnToolbarGUI;

    static IMGUITEST()
    {
        EditorApplication.update -= OnUpdate;
        EditorApplication.update += OnUpdate;
    }

    static void OnUpdate()
    {
        // Relying on the fact that toolbar is ScriptableObject and gets deleted when layout changes
        if (m_currentToolbar == null)
        {
            //获取到原有的Toolbar
            var toolbars = Resources.FindObjectsOfTypeAll(m_toolbarType);
            m_currentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
            if (m_currentToolbar != null)
            {
                //获取Toolbar的VisualTree
                var visualTree = (VisualElement)m_viewVisualTree.GetValue(m_currentToolbar, null);

                // Get first child which 'happens' to be toolbar IMGUIContainer
                var container = (IMGUIContainer)visualTree[0];

                //重新注册handler
                var handler = (Action)m_imguiContainerOnGui.GetValue(container);
                handler -= OnGUI;
                handler += OnGUI;
                m_imguiContainerOnGui.SetValue(container, handler);
            }
        }
    }

    //自定义的绘制函数
    static void OnGUI()
    {
        var handler = OnToolbarGUI;
        if (handler != null) handler();
    }
}
