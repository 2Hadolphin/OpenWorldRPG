using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Return.Items;
using Return.InteractSystem;

public class ItemAroundList : MonoBehaviour
{
    public GameObject ElementPrefab;
    private RectTransform rt;
    public GameObject slot;
    int CurSlot;
    [SerializeField]
    private GameObject mask = null;

    //[SerializeField]
    //private SelectionUGUI SelectAS = null;

    private void Start()
    {
        rt = gameObject.GetComponent<RectTransform>();
        mask = gameObject.transform.GetChild(1).gameObject;
        
    }
    private void OnEnable()
    {
        
    }
    /*
    public void LoadingAssets()
    {
        int menbers =SelectAS.ZoneObject.Count;
        CurSlot = slot.transform.childCount;
        //print(CurSlot+"/"+menbers);
        if (menbers-CurSlot>0)
        {
            for (int slotNum = CurSlot; slotNum < menbers ; slotNum++)
            {
                GameObject cage = Instantiate(ElementPrefab);
                cage.transform.SetParent(slot.transform, false);
                cage.SetActive(true) ;
                //print("Add Slot ");
            }
            CurSlot = slot.transform.childCount;
            //print("Slots has been add to "+CurSlot);
        }
        else if (menbers - CurSlot < 0)
        {
            for (int i=0 ; i< CurSlot; i++)
            {
                Destroy(slot.transform.GetChild(i).CharacterRoot);
                //print("Destory" + slot.transform.GetChild(i).CharacterRoot);
            }

        }
        else
        {
            
        }
        //print(menbers);
        RefreshAssets(menbers);

        if (menbers == 0)
        {
            mask.GetComponent<Mask>().showMaskGraphic = false;
        }
        else
        {
            mask.GetComponent<Mask>().showMaskGraphic = true;
            mask.GetComponent<RectTransform>().sizeDelta = new Vector2(100, menbers * 30);
        }
        
    }

    private void UnRefAssets()
    {
       
    }
    private void OnDisable()
    {
        //UnRefAssets();
    }

    public void RefreshAssets(int menbers)
    {
        for (int i = 0; i < menbers ; i++)
        {
            slot.transform.GetChild(i).GetComponent<ItemListCage>().Sprite.sprite = SelectAS.ZoneObject[i].GetComponent<m_items>().GetData.Sprite;
            slot.transform.GetChild(i).GetComponent<ItemListCage>().Name.text = SelectAS.ZoneObject[i].GetComponent<m_items>().GetName;
            slot.transform.GetChild(i).GetComponent<ItemListCage>().m_items = SelectAS.ZoneObject[i];
        }
        
    }
    */
}
