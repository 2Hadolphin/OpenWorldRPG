using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.Assertions;
using System.Linq;

public class BindingInput : MonoBehaviour
{
    public InputActionAsset mAction;
    [SerializeField]
    private InputBinding.DisplayStringOptions m_DisplayStringOptions;

    public RebindActionUI Option;
    [Tooltip("Optional UI that will be shown while a rebind is in progress.")]
    [SerializeField]
    private GameObject m_RebindOverlay;

    [Tooltip("Optional text label that will be updated with prompt for user Input.")]
    [SerializeField]
    private TextMeshProUGUI m_RebindText;

    

    private void Start()
    {
        LoadUI();
    }

    public virtual void StartBindKey()
    {
        // show binding page & blur
        Debug.Log("binding");
    }

    void LoadUI()
    {


        if (mAction == null)
        {
            var targets = Resources.LoadAll<InputActionReference>(string.Empty);
            var length = targets.Length;

            var controlConfigs = new List<SettingConfig>(length);
            var title = string.Empty;
            for (int i = 0; i < length; i++)
            {
                var action = targets[i].action;

                if(action.actionMap.name != title)
                {
                    title = action.actionMap.name;
                    controlConfigs.Add(new TitleConfig() { Title = title });
                }

                var displayString = string.Empty;
                var deviceLayoutName = default(string);
                var controlPath = default(string);

                // Get display string from action.
                if (action != null)
                {
                    //var bindingIndex = action.bindings.GetChildOfIndex(x => x.id.ToString() == m_BindingId);
                    //if (bindingIndex != -1)
                        displayString = action.GetBindingDisplayString(0, out deviceLayoutName, out controlPath, m_DisplayStringOptions);
                }

                // Give listeners a chance to configure UI in response.
                //m_UpdateBindingUIEvent?.Invoke(this, displayString, deviceLayoutName, controlPath);

                controlConfigs.Add(
                    new ButtonConfig()
                    {
                        Title = action.name,
                        Buttons = new[]
                         {
                            new ButtonConfig.ButtonConfigs()
                            {
                            BindingKey=displayString,
                            Callback=StartBindKey,
                            }
                        },
                    });
            }

            //MainSettingUIManager.AddConfigs(MainSettingUIManager.SettingType.Control, "Inputs Binding", controlConfigs.ToArray());

        }
        else
        {
            var inputActions = mAction.GetEnumerator();

            var controlConfigs = new List<SettingConfig>();
            var title = string.Empty;


            while (inputActions.MoveNext())
            {
                var action = inputActions.Current;

                Assert.IsNotNull(action);

                if (action.actionMap.name != title)
                {
                    title = action.actionMap.name;
                    controlConfigs.Add(new TitleConfig() { Title = title });
                }

                var displayString = string.Empty;
                var deviceLayoutName = default(string);
                var controlPath = default(string);

                // Get display string from action.
                if (action != null)
                {
                    //var bindingIndex = action.bindings.GetChildOfIndex(x => x.id.ToString() == m_BindingId);
                    //if (bindingIndex != -1)
                    displayString = action.GetBindingDisplayString(0, out deviceLayoutName, out controlPath, m_DisplayStringOptions);
                }

                //Assert.IsNotNull(action.activeControl);
                var config = new ButtonConfig()
                {
                    Title = action.name,
                    Buttons = new[]
                    {
                            new ButtonConfig.ButtonConfigs()
                            {
                            BindingKey=displayString,//string.Join('-',action.bindings.Select(x=>x.action)),
                            Callback=StartBindKey,
                            }
                     },
                };

                controlConfigs.Add(config);
            }

            //MainSettingUIManager.AddConfigs(MainSettingUIManager.SettingType.Control, "Inputs Binding", controlConfigs.ToArray());
        }

//        var controlConfigs = new SettingConfig[]
//{
//            new ButtonConfig() { Title = "Movement", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback= StartBindKey, BindingKey= "W-A-S-D" } } },
//            new ButtonConfig() { Title = "Jump", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback= StartBindKey, BindingKey= "Space" } } },
//            new ButtonConfig() { Title = "Crouch", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback= StartBindKey, BindingKey= "Left-Ctrl" } } },
//            new ButtonConfig() { Title = "Climb", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback= StartBindKey, BindingKey= "Left-Alt" } } },
//            new ButtonConfig() { Title = "Sprint", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback= StartBindKey, BindingKey= "Left-Shift" } } },
//            new ButtonConfig() { Title = "Idle", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback= StartBindKey, BindingKey= "~" } } },
//            new ButtonConfig() { Title = "Control-AA", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback= StartBindKey, BindingKey= "AA" } } },
//            new ButtonConfig() { Title = "Control-BB", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback= StartBindKey, BindingKey= "BB" } } },
//            new ButtonConfig() { Title = "Control-CC", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback= StartBindKey, BindingKey= "CC" } } },
//            new ButtonConfig() { Title = "Control-DD", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback= StartBindKey, BindingKey= "DD" } } },
//            new ButtonConfig() { Title = "Control-EE", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback= StartBindKey, BindingKey= "EE" } } },
//};
    }
}
