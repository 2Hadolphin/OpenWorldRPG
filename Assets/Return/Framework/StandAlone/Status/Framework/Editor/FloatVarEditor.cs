using UnityEditor;

namespace Return.Framework.Stats
{
    [CanEditMultipleObjects, CustomEditor(typeof(FloatVar))]
    public class FloatVarEditor : VariableEditor
    {
        public override void OnInspectorGUI() => PaintInspectorGUI("Float Variable");
    }
}