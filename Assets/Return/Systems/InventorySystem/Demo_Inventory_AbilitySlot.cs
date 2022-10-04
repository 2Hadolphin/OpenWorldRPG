using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class Demo_Inventory_AbilitySlot : MonoBehaviour
{
    public TextMeshProUGUI ID;
    public TextMeshProUGUI Exp;
    public TextMeshProUGUI Level;

    public Slider Ability_Health;
    public Slider Ability_Strength;
    public Slider Ability_Agility;
    public Slider Ability_Armor;

    public void OnDataUpdate()
    {
        var data=DemoManager.Instance.CurrentData;

        ID.text = data.Name;
        Level.text = data.Level.ToString();
        Exp.text = data.EXP.ToString();
        var max = 100f;

        if (data.GetAbility(mAbility.Health, out var ability))
            Ability_Health.value = ability.Value / max;
        if (data.GetAbility(mAbility.Strength, out  ability))
            Ability_Strength.value = ability.Value / max;
        if (data.GetAbility(mAbility.Agility, out  ability))
            Ability_Agility.value = ability.Value / max; 
        if (data.GetAbility(mAbility.Armor, out  ability))
            Ability_Armor.value = ability.Value / max;
    }

}
