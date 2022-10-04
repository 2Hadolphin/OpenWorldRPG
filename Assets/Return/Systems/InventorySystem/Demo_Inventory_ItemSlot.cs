using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Return;


[SerializeField]
public class Demo_Inventory_ItemSlot : BaseComponent, IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler
{
    public int Code;
    public static float ToolTipTime =0.31f;

    public bool Empty = true;


    public int Index;
    public Image Icon;
    public Image Background;
    public Image Frame;
    public TextMeshProUGUI Amount;
    public GameObject Notify;

    public void SetIcon(Sprite sprite)
    {
        if (sprite)
        {
            Icon.sprite = sprite;
            Icon.color = Color.white;
            Empty = false;
        }
        else
        {
            Icon.sprite = null;
            Icon.color = Color.clear;
            Empty = true;
        }
    }
    public void CopyStyle(Demo_Inventory_ItemSlot sample)
    {
        Background.color = sample.Background.color;
        Frame.sprite = sample.Frame.sprite;
        Amount.text = string.Empty;

        Notify.SetActive(false);

    }

    public void Disable()
    {
        Icon.raycastTarget = false;
        Background.raycastTarget = false;

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                if(eventData.clickCount>1)
                    Message("Use");
                else
                    Message("Select");
                break;
            case PointerEventData.InputButton.Right:
                Message("Dequip");
                break;
            case PointerEventData.InputButton.Middle:

                break;
        }
    }


    void Message(string txt)
    {
        SendMessageUpwards(txt, Code);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Message("Release");
    }

    public void OnDrop(PointerEventData eventData)
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Empty)
            return;
        StartCoroutine(StayCountdown());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        Message("CloseToolTip");
    }

    IEnumerator StayCountdown()
    {
        yield return new WaitForSeconds(ToolTipTime);
        Message("ToolTip");
        yield break;
    }
}
