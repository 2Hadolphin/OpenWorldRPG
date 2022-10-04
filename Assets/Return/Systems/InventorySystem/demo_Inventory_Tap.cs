//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Events;
//using UnityEngine.UI;

//using Return;
//using Return.m_items;

//public class demo_Inventory_Tap : MonoBehaviour
//{
//    public demo_Inventory inventory;
//    public Button Tap_All;
//    public Button Tap_Weapon;
//    public Button Tap_Armor;
//    public Button Tap_Equipment;
//    public Button Tap_Consumable;
//    public RectTransform Force;
//    public AudioClip Effect;

//    public ItemType LastType;
//    private void Start()
//    {
//        set();
//    }

//    void On_Button(ItemType type)
//    {
//        if (LastType == type)
//            return;

//        switch (LastType)
//        {
//            case ItemType.Weaopn:
//                Tap_Weapon.interactable = true;

//                break;
//            case ItemType.Armor:
//                Tap_Armor.interactable = true;

//                break;
//            case ItemType.Equipment:
//                Tap_Equipment.interactable = true;

//                break;
//            case ItemType.Consumable:
//                Tap_Consumable.interactable = true;

//                break;
//            case ItemType.Default:
//                Tap_All.interactable = true;
//                break;
//        }
//        LastType = type;

//        inventory.SwitchTab(type);
//    }

//    void On_All()
//    {
//        On_Button(ItemType.Default);
//    }
//    void On_Weapon()
//    {
//        On_Button(ItemType.Weaopn);
//    }

//    void On_Armor()
//    {
//        On_Button(ItemType.Armor);
//    }
//    void On_Equipment()
//    {
//        On_Button(ItemType.Equipment);
//    }
//    void On_Consumable()
//    {
//        On_Button(ItemType.Consumable);
//    }

//    void SetForce(ReadOnlyTransform transform)
//    {
//        mAudioManager.PlayEffect(Effect);
//        Force.SetParent(transform);
//        Force.SetRight(0);
//        Force.SetLeft(0);


//    }





//    public void set()
//    {
//        SetButton(Tap_All, new UnityAction(On_All));
//        SetButton(Tap_Weapon, new UnityAction(On_Weapon));
//        SetButton(Tap_Armor, new UnityAction(On_Armor));
//        SetButton(Tap_Equipment, new UnityAction(On_Equipment));
//        SetButton(Tap_Consumable, new UnityAction(On_Consumable));

//        Tap_All.onClick.Invoke();
//    }

//    void SetButton(Button button, UnityAction action)
//    {
//        var action1 = new UnityAction(() => SetForce(button.transform));
//        var action2 = new UnityAction(() => button.interactable = false);


//        button.onClick.AddListener(action);
//        button.onClick.AddListener(action1);
//        button.onClick.AddListener(action2);
//    }
//}
