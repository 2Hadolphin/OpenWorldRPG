using UnityEditor;

namespace Return.Framework.Stats
{
    [CanEditMultipleObjects, CustomEditor(typeof(BoolVar))]
    public class BoolVarEditor : VariableEditor
    {
        public override void OnInspectorGUI() => PaintInspectorGUI("Bool Variable");
    }
}
