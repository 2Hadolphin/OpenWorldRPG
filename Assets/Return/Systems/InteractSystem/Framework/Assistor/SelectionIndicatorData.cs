using UnityEngine;

namespace Return.InteractSystem
{
    public class SelectionIndicatorData : BaseSelectionIndicatorData
    {
        //public string Title { get; set; }
        public string Context { get; set; }

        public override void DrawGUI(Rect rect)
        {
            GUI.Label(rect, Context);
        }
    }
}