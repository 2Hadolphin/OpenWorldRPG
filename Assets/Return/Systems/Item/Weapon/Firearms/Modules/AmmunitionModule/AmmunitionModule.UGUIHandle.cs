using UnityEngine;

namespace Return.Items.Weapons
{
    public partial class AmmunitionModule
    {
        public class UGUIHandle : ItemModuleHandle<AmmunitionModule>
        {
            

            protected override void OnEnable()
            {
                Routine.GUI.Subscribe(OnGUI);
            }

            protected override void OnDisable()
            {
                Routine.GUI.Unsubscribe(OnGUI);
            }

            GUIStyle style;

            [SerializeField]
            Vector2 LabelSize = new(300, 120);

            private void OnGUI()
            {
                if (style.IsNull())
                {
                    style = new(GUI.skin.label);
                    style.fontSize = 35;
                    style.alignment = TextAnchor.LowerRight;
                }

                var mag = Module.Magazine;

                if (mag.IsNull())
                    return;

                var msg = string.Format("{0} / {1}", mag.Amount, mag.Capacity);

                var rect = new Rect(Screen.width - LabelSize.x, Screen.height - LabelSize.y, LabelSize.x, LabelSize.y);

                GUI.Label(rect, msg, style);
            }
        }

        //#endregion
    }
}