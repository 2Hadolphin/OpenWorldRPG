using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Return.Items;
using Return;

public class demo_Inventory : MonoBehaviour//, ICursorHandler
{
    //public CanvasGroup CanvasGroup;

    //public ItemArchives Archives;



    //public AudioClip Effect_OpenClose;
    //public Canvas Canvas;

    //public Demo_Inventory_AbilitySlot Demo_AbilitySlot;

    //[BoxGroup("Slot")]
    //public int SlotAmount = 54;
    //[BoxGroup("Slot")]
    //public GameObject Prefab_InventorySlot;
    //[BoxGroup("Slot")]
    //public RectTransform SlotHandler;
    //[BoxGroup("Slot")]// attach to Rarity
    //public Demo_Inventory_ItemSlot[] SlotStyle=new Demo_Inventory_ItemSlot[Enum.GetValues(typeof(RarityLevel)).Length];
    //[ReadOnly]
    //[BoxGroup("Slot")]
    //public List<Demo_Inventory_ItemSlot> Slots;
    //[BoxGroup("Slot")]
    //public List<Demo_Inventory_ItemSlot> DisableSlots=new List<Demo_Inventory_ItemSlot>(6);
    //[BoxGroup("Slot")]
    //public AudioClip SelectedEffect;
    //[BoxGroup("Slot")]
    //public AudioClip DropEffect;
    //[BoxGroup("Slot")]
    //public Sprite DragImage;
    //[BoxGroup("Slot")]
    //[ReadOnly]
    //public bool isDrag = false;
    //[BoxGroup("Slot")]
    //public SlotHandler ClampBag;
    
    //bool HasTargeted
    //{
    //    get
    //    {
    //        return ClampBag.Inside;
    //    }
    //}



    //[SerializeField]
    //public ItemArchives Env;

    //Dictionary<int, ItemInfo> slotCatch;
    //public class Bag
    //{
    //    public Bag(int Capacity)
    //    {
    //        m_items = new List<ItemPreset>(Capacity);
    //    }
    //    public int Capacity { get => m_items.Count; }


    //    protected List<ItemPreset> m_items;
    //    public IList<ItemPreset> GetItems { get => m_items; }
    
    //}

    //public void Dequip(int key)
    //{
    //    if (slotCatch.TryGetValue(key, out var item))
    //        Release(Slots[key]);
    //}

    //void Release(Demo_Inventory_ItemSlot slot)
    //{
    //    if (slotCatch.TryGetValue(slot.Code, out var item))
    //        slotCatch.Remove(slot.Code);


    //    Archives.Remove(item);

    //    CleanSlot(slot);

    //    var tf = DemoManager.m_Instance.Agent.root;
    //    var go=Instantiate(item.Model, tf.position,tf.rotation);
    //    //go.AddComponent<ItemShowcase>().Preset=item;

    //}

    //public Demo_Inventory_ItemSlot SelectedSlot;
    //[ReadOnly][BoxGroup("Slot")]
    //public bool IsTooltip;
    //int tooltipSn=-1;
    ////public demoItemTooltip ToolTipHandler;
    //public void ToolTip(int key)
    //{
    //    if (slotCatch.TryGetValue(key, out var tooltipData))
    //    {
    //        SetTooltip(tooltipData);
    //    }
    //    else
    //    {
    //        Debug.LogError("");
    //    }
    //    tooltipSn = key;
    //}



    //void SetTooltip(Return.m_items.ItemInfo data)
    //{
    //    ToolTipHandler.enabled = true;
    //    ToolTipHandler.RegisterHandler(data);
    //    ToolTipHandler.CopyStyle(GetStyle(data.Rarity));
    //}

    //public void CloseToolTip(int key)
    //{
    //    if (key != tooltipSn)
    //        return;
    //    ToolTipHandler.enabled = false;
    //    tooltipSn = -1;
    //}

    //public void Select(int Key)
    //{
        
    //    var slot = Slots[Key];
    //    if (!isDrag) //ondrag
    //    {
    //        if (!slot.Empty)
    //        {
    //            isDrag = true;

    //            SubscribeInput = isDrag;
    //            StartCoroutine(DragAttach(slot));
    //        }

    //    }
    //    else
    //    {
    //        isDrag = false;
    //    }
    //    SelectedSlot = slot;
    //}



    //public void MouseDown_Right(InputAction.CallbackContext ctx) //cancel
    //{
    //    isDrag = false;
    //    SubscribeInput = false;
    //}
    //public void MouseDown_Left(InputAction.CallbackContext ctx)
    //{
    //    if (!ClampBag.Inside)
    //        isDrag = false;
  
    //    SubscribeInput = false;

    //    if (!HasTargeted) // go Schedule_Drop.give.use
    //    {
    //        var position = Mouse.current.position.ReadValue();

    //        if(SelectedSlot)
    //            Release(SelectedSlot);

    //    }
    //}


    //bool SubscribeInput
    //{
    //    set
    //    {
    //        var Input = InputManager.Inputs;

    //        if (value)
    //        {
    //            Input.GUI.CursorLeft.performed += MouseDown_Left;
    //            Input.GUI.CursorRight.performed += MouseDown_Right;
    //        }
    //        else
    //        {
    //            Input.GUI.CursorRight.performed -= MouseDown_Right;
    //            Input.GUI.CursorLeft.performed -= MouseDown_Left;
    //        }
    //    }
    //}

    //Demo_Inventory_ItemSlot GetStyle(RarityLevel rarity)
    //{
    //    return SlotStyle[(int)rarity];
    //}

    //public void SwitchTab(ItemType type)
    //{
    //    BagType = type;
    //    UpdateSlot();
    //}
    //ItemType BagType;
    //void UpdateSlot()
    //{

    //    var length = Slots.Count;
    //    var style = GetStyle(RarityLevel.Normal);

    //    for (int i = 0; i < length; i++)
    //    {
    //        Slots[i].CopyStyle(style);
    //        Slots[i].SetIcon(null);
    //    }

    //    slotCatch.Clear();

    //    var storage = ItemArchives.Instance.GetEnumerator();
    //    var sn = 0;

    //    if (BagType == ItemType.Default)
    //    {
    //        while (storage.MoveNext())
    //        {
    //            SetSlot(Slots[sn], storage.Current);
    //            sn++;
    //        }
    //    }
    //    else
    //        while (storage.MoveNext())
    //        {
    //            if (BagType.Equals(storage.Current.m_category))
    //            {
    //                SetSlot(Slots[sn], storage.Current);
    //                sn++;
    //            }
    //        }
    //}

    //void SetSlot(Demo_Inventory_ItemSlot slot,Return.m_items.ItemInfo item)
    //{
    //            slot.SetIcon(item.Sprite);
    //            slot.CopyStyle(SlotStyle[(int)(item.Rarity)]);
    //            slotCatch.Add(slot.Code, item);
    //}

    //void CleanSlot(Demo_Inventory_ItemSlot slot)
    //{
    //    var style = SlotStyle[(int)RarityLevel.Normal];

    //    slot.CopyStyle(style);
    //    slot.SetIcon(null);
    //}

    //IList<ItemPreset> GetBag(ItemType type)
    //{
    //    return type switch
    //    {
    //        ItemType.Weaopn => Weaopn.GetItems,
    //        ItemType.Armor => Armor.GetItems,
    //        ItemType.Equipment => Equipment.GetItems,
    //        ItemType.Consumable => Consumable.GetItems,
    //        ItemType.Default=>All.GetItems,
    //        _ => throw new KeyNotFoundException(),
    //    };
    //}
    


    //public Bag Weaopn=new Bag(40);
    //public Bag Armor = new Bag(40);
    //public Bag Equipment = new Bag(40);
    //public Bag Consumable = new Bag(80);
    //public Bag All = new Bag(200);

    //public bool GetCursorLock => !Canvas.enabled;

    //public bool IgnoreLock =>  false;

    //public void IArchiveContent(Return.m_items.ItemInfo data)
    //{
    //    Archives.Add(data);
    //    UpdateSlot();
    //}

    //IEnumerator DragAttach(Demo_Inventory_ItemSlot draggingSlot)
    //{
    //    var tf = DragImage.rectTransform;
    //    DragImage.sprite = draggingSlot.Sprite.sprite;

    //    DragImage.enabled = true;

    //    mAudioManager.PlayEffect(SelectedEffect);
    //    var control = Mouse.current.position;
    //    while (isDrag&&enabled)
    //    {
    //        tf.position = control.ReadValue();
    //        yield return null;
    //    }

    //    if (enabled)
    //    {
    //        if (SelectedSlot != draggingSlot)
    //        {
    //            SwitchSlotContain(SelectedSlot, draggingSlot);
    //        }
    //        mAudioManager.PlayEffect(SelectedEffect);
    //    }

    //    DragImage.enabled = false;
    //    yield break;
    //}

    //public void Use(int sn)
    //{
    //    if(slotCatch.TryGetValue(sn,out var item))
    //    {
    //        isDrag = false;
    //        var agent = DemoManager.m_Instance.Agent;
    //        agent.ItemUsingSystem.Equip(item);
    //        var slot = Slots[sn];
    //        slotCatch.Remove(slot.Code);
    //        Archives.Remove(item);
    //        CleanSlot(slot);
    //        CloseToolTip(tooltipSn);
    //    }
    //}

    //void SwitchSlotContain(Demo_Inventory_ItemSlot a, Demo_Inventory_ItemSlot b)
    //{
    //    Return.m_items.ItemInfo item_a=default;
    //    Return.m_items.ItemInfo item_b = default;
    //    var emptyA = a.Empty?a.Empty: !slotCatch.TryGetValue(a.Code, out item_a);
    //    var emptyB = b.Empty ? b.Empty : !slotCatch.TryGetValue(b.Code, out item_b);

    //    if (!emptyA)
    //        slotCatch.Remove(a.Code);

    //    if (!emptyB)
    //        slotCatch.Remove(b.Code);

    //    if (!emptyA)
    //        SetSlot(b, item_a);
    //    else
    //        CleanSlot(b);

    //    if (!emptyB)
    //        SetSlot(a, item_b);
    //    else
    //        CleanSlot(a);
    //}

    //private void Awake()
    //{

    //    var Input = InputManager.Inputs;
    //    Input.GUI.InventoryHandler.performed += Switch;
    //    Input.Battle.Switch.performed += QuickSlot;
    //    slotCatch = new Dictionary<int, Return.m_items.ItemInfo>(SlotAmount);
    //    Slots = new List<Demo_Inventory_ItemSlot>(SlotAmount);
        
    //    for (int i = 0; i < SlotAmount; i++)
    //    {
    //        var slot = Instantiate(Prefab_InventorySlot, SlotHandler);
    //        Slots.Add( slot.GetComponent<Demo_Inventory_ItemSlot>());
    //        Slots[i].Code = i;
    //    }
    //    var filledAmount = SlotFilledAmount;
    //    for (int i = 0; i < 6; i++)
    //    {
    //        var slot = Instantiate(Prefab_InventorySlot, SlotHandler).GetComponent<Demo_Inventory_ItemSlot>();
    //        slot.CopyStyle(SlotStyle[(int)RarityLevel.Disable]);
    //        slot.Disable();
    //        DisableSlots.Add(slot);
    //    }

    //    SetDisableSlot(filledAmount);

    //    CursorController.RegisterHandler(this);
    //}

    //void SetDisableSlot(int num)
    //{
    //    var length = DisableSlots.Count;
    //    for (int i = 0; i < length; i++)
    //    {
    //        if (i < num)
    //            DisableSlots[i].CharacterRoot.SetActive(true);
    //        else
    //            DisableSlots[i].CharacterRoot.SetActive(false);
    //    }
    //}

    //public int SlotFilledAmount => 6 - SlotAmount % 6;



    //void Switch(InputAction.CallbackContext ctx)
    //{
    //    if (enabled)
    //        Close();
    //    else
    //        enabled = true;

    //    mAudioManager.PlayEffect(Effect_OpenClose);
    //}
    //int quickSn = 0;
    //void QuickSlot(InputAction.CallbackContext ctx)
    //{
    //    if (slotCatch.Count == 0)
    //        return;

    //    var keys = slotCatch.Keys.ToArray();
    //    quickSn = keys.Loop(quickSn + 1);

    //    Use(keys[quickSn]);
    //}

    //private void OnEnable()
    //{
    //    CanvasGroup.interactable = true;
    //    CanvasGroup.alpha = 1;
    //    Canvas.enabled = true;
    //    CursorController.SetCursorLock(false);
    //    Demo_AbilitySlot.OnDataUpdate();

    //}
    //private void OnDisable()
    //{ 

    //    CanvasGroup.interactable = false;
    //    CanvasGroup.alpha = 0;
    //    Canvas.enabled = false;
    //    ToolTipHandler.enabled = false;
    //    CursorController.SetCursorLock(true);
    //}

    //private void Start()
    //{
    //    Archives = ItemArchives.Instance;

    //    Close();
    //}
    //public void Close()
    //{
    //    enabled = false;
    //}
}
