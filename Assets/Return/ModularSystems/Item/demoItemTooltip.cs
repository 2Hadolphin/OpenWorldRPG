using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class demoItemTooltip : MonoBehaviour
{
    [SerializeField]
    public Canvas Canvas;
    [SerializeField]
    public Image Icon;
    [SerializeField]
    public TextMeshProUGUI Name,Type,Description;

    public Image Frame,IconBackground;
    RectTransform rt;
    void Awake()
    {
        rt = GetComponent<RectTransform>();
        enabled = false;
    }

    public void Register(Return.Items.ItemInfo data)
    {
        Name.text = data.Name;
        Type.text = data.Rarity.ToString();
        Description.text = data.Description;
        Icon.sprite = data.Icon.Sprite;
    }

    public void CopyStyle(Demo_Inventory_ItemSlot sample)
    {
        IconBackground.color = sample.Background.color;
        Frame.sprite = sample.Frame.sprite;
    }

    private void OnEnable()
    {
        Canvas.enabled = true;

    }
    private void OnDisable()
    {
        Canvas.enabled = false;

    }


    void LateUpdate()
    {
        var control = Mouse.current.position;
        rt.position = control.ReadValue();
    }
}
