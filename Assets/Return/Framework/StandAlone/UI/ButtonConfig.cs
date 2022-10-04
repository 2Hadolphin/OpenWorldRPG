using System;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class ButtonConfig : SettingConfig, INormalField
{
    [System.Serializable]
    public struct ButtonConfigs
    {
        public string BindingKey;
        public Action Callback;
    }

    public ButtonConfigs[] Buttons;

    public virtual void Apply(Button button, TextMeshProUGUI textMeshPro, int sn = 0)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(new UnityAction(Buttons[sn].Callback));
        textMeshPro.text = Buttons[sn].BindingKey;
    }
}
