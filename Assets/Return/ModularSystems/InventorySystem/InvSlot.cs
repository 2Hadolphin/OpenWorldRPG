using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Return.Items;
using System;


[Obsolete]
public class InvSlot : MonoBehaviour
{
    //public string ID;
    //protected IItem _Item;
    //public IItem GetItem
    //{
    //    get
    //    {
    //        if (null == _Item && transform.childCount > 0)
    //            transform.GetChild(0).TryGetComponent(out _Item);

    //        return _Item;
    //    }
    //}


    //public int Priority(IItem item)
    //{
    //    return (int)item.Preset.Rarity;
    //}

    //public int SlotNumber;
    //public GameObject m_items;

    //public string Name, UID, Type, Description;
    

    //private int ClickCount=0;
    //private float timmer;
    //private bool yeldClick;

    //public GameObject Sprite;
    //[SerializeField]
    //private bool empty=true;

    //private void Start()
    //{
    //    //itembag = CharacterRoot.GetComponentInParent<ItemBag>();
    //}

    //public bool Empty
    //{ // This is a property.
    //    get
    //    {
    //        return empty;
    //    }
    //    set
    //    {
    //        empty = value; // Note the use of the implicit variable "value" here.
    //    }
    //}

    
    //public void goReleaseItem()
    //{
    //    if (!empty)
    //    {
    //        //itembag.Release(SlotNumber);
    //    }
    //}

    //public void CleanSlot()
    //{
    //    if (!empty)
    //    {
    //        m_items = null;
    //        Name = null;
    //        Type = null;
    //        Description = null;
    //        UID = null;
    //        empty = true;
    //    }
    //}

    ////ButtonControll
    //public void OnClick()
    //{
    //    if (m_items != null)
    //    {
    //        //print("Clicked");
    //        if (ClickCount == 1)
    //        {
    //            ClickCount++;
    //        }
    //        else if (ClickCount == 0)
    //        {
    //            ClickCount++;

    //        }
    //    }
    //}

    //private IEnumerator ClickTimmer()
    //{
    //    timmer = Time.time;
    //    yeldClick = true;

    //    yield return new WaitForSeconds(0.3f);
    //    if (ClickCount == 1 && yeldClick == true)
    //    {
    //        SelectItem();
    //        ClickCount = 0;
    //        yeldClick = false;
    //        yield break;
    //    }
    //    else if (ClickCount == 2 && yeldClick == true)
    //    {
    //        UseItem();
    //        ClickCount = 0;
    //        yeldClick = false;
    //        yield break;
    //    }
    //    else

    //    yield break;

    //}



    //public void UseItem()
    //{
    //    //itembag.SwitchPrime(SlotNumber);
    //}
    //public void SelectItem()
    //{
    //}

}
