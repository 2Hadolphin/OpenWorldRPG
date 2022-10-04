
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;


// Tagging a class with the EditorTool attribute and no target type registers a global tool. Global tools are valid for any selection, and are accessible through the top left toolbar in the editor.
[EditorTool("CamFollow Tool")]
class Tool_CamFollow : EditorTool
{
    // Serialize this value to set a default value in the Inspector.
    [SerializeField]
    Texture2D m_ToolIcon;

    GUIContent m_IconContent;

    public override void OnActivated()
    {
        base.OnActivated();

        SceneView.beforeSceneGui -= SceneView_beforeSceneGui;
        SceneView.beforeSceneGui += SceneView_beforeSceneGui;

        Debug.Log("Active");
    }



    public override void OnWillBeDeactivated()
    {
        Debug.Log("Disable");
        SceneView.beforeSceneGui -= SceneView_beforeSceneGui;
    }

    void OnEnable()
    {
        m_IconContent = new GUIContent()
        {
            image = m_ToolIcon,
            text = "CamFollow Tool",
            tooltip = "Make scene camera follow avtive gameobject"
        };
    }

    private void OnDisable()
    {

    }

    Transform cam;

    private void SceneView_beforeSceneGui(SceneView view)
    {

        cam = view.camera.transform;

        var target = Selection.activeGameObject;

        if (target == null)
            return;

        if (cam == null)
            return;

        cam.LookAt(target.transform, Vector3.up);
    }



    public override GUIContent toolbarIcon
    {
        get { return m_IconContent; }
    }

    // This is called for each window that your tool is active in. Put the functionality of your tool here.
    public override void OnToolGUI(EditorWindow window)
    {
        EditorGUI.BeginChangeCheck();

        Vector3 position = Tools.handlePosition;
        Quaternion rotation = Tools.handleRotation;

        using (new Handles.DrawingScope(Color.green))
        {
            position = Handles.Slider(position, Vector3.right);
            rotation = Handles.RotationHandle(rotation, position);
        }

        if (cam == null)
            return;

        var e = Event.current;

        if (e.type.HasFlag(EventType.ScrollWheel) && e.isScrollWheel)
        {
            var scroll = e.delta.y;

            cam.Translate(Vector3.forward * (scroll * 1f), Space.Self);

            e.Use();
        }

        if (EditorGUI.EndChangeCheck())
        {
            //if(rotation!= Tools.handleRotation)
            {
                var angles = rotation.eulerAngles;
                cam.RotateAround(position, Vector3.right, angles.x);
                cam.RotateAround(position, Vector3.up, angles.y);
                cam.RotateAround(position, Vector3.forward, angles.z);
            }

            //Selection.activeTransform.position = position;
            //SceneView.currentDrawingSceneView.camera.transform.LookAt(Selection.activeTransform, Vector3.up);
        }
    }
}