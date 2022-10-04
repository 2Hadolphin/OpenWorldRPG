using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Return;

public class BindingConfig : ButtonConfig 
{ 
    public override void Apply(Button button, TextMeshProUGUI textMeshPro, int sn = 0)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(new UnityAction(Buttons[sn].Callback));
        textMeshPro.text = Buttons[sn].BindingKey;


        //Rebind.m_Action=
        //not need binding ui Rebind.m_RebindOverlay
        //title field? Rebind.m_RebindText =
        return;
        button.gameObject.InstanceIfNull(ref Rebind);
        Rebind.UpdateBindingID();
        Rebind.UpdateActionLabel();
    }



    protected RebindActionUI Rebind;

    static void UpdateBinding()
    {


        //foreach (var inputAction in targets)
        //{
        //    var option = Instantiate(Option, transform);

        //    option.m_Action = inputAction;
        //    option.m_RebindOverlay = m_RebindOverlay;
        //    option.m_RebindText = m_RebindText;
        //    option.UpdateBindingID();
        //    option.UpdateActionLabel();
        //}
    }
}
