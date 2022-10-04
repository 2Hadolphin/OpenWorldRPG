using UnityEngine;
using UnityEngine.InputSystem;

public class RebindSaveLoad : MonoBehaviour
{
    public InputActionAsset actions;
    public const string Prefs = "mInputBinds";
    public void OnEnable()
    {
        var rebinds = PlayerPrefs.GetString(Prefs);
        if (!string.IsNullOrEmpty(rebinds))
            actions.LoadBindingOverridesFromJson(rebinds);
    }


    public void OnDisable()
    {
        var rebinds = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(Prefs, rebinds);
    }
}
