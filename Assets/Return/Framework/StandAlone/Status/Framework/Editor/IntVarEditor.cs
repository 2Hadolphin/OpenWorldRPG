using UnityEditor;

namespace Return.Framework.Stats
{

    [CanEditMultipleObjects, CustomEditor(typeof(IntVar))]
    public class IntVarEditor : VariableEditor
    {
        public override void OnInspectorGUI() => PaintInspectorGUI("Int Variable");
    }
}