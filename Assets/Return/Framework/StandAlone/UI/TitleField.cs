using UnityEngine;
using TMPro;
using EnhancedUI.EnhancedScroller;
using System;

public class TitleField : EnhancedScrollerCellView
{
    [SerializeField]
    public TextMeshProUGUI Title;


    public event Action<int,int> ApplyControl;
    public event Action OnReleaseCellView;

    public virtual void SetData(SettingConfig config)
    {
        Title.text = config.Title;
        gameObject.name = config.Title;
    }

    //public virtual void ResetField(string title = default)
    //{
    //    Title.text = title;
    //}

    private void OnEnable()
    {
        //ApplyControl.Invoke(cellIndex,dataIndex);
    }

    private void OnDisable()
    {
        //Debug.Log(active);
        //Release();
    }

    public override void RefreshCellView()
    {
        //Debug.LogError("Refresh : "+dataIndex + Title.text);
        //Release();
    }

    public virtual void Release()
    {
        //Debug.Log("Release : "+dataIndex + gameObject.name);
        OnReleaseCellView?.Invoke();
        OnReleaseCellView = null;
    }

}

