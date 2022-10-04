using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

namespace Return.UI
{
    //[RequireComponent(typeof(RectTransform))]
    public class OnPointerClick : BaseComponent, IPointerClickHandler
    {
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            OnPointerClickEvent?.Invoke(eventData);
        }


        [SerializeField]
        PointerArg m_OnPointerClickEvent;
        public PointerArg OnPointerClickEvent { get => m_OnPointerClickEvent; set => m_OnPointerClickEvent = value; }


    }

    [Serializable]
    public class PointerArg : UnityEvent<PointerEventData> { }
}