using UnityEngine;

namespace Return.InteractSystem
{
    public abstract class BaseSelectionIndicatorData
    {
        public ICoordinate DrawPosition;
        public abstract void DrawGUI(Rect rect);
    }
}