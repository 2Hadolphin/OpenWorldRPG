using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace Return.UI
{
    /// <summary>
    /// ??
    /// </summary>
    public class CustomWindow : BaseComponent, ISelectHandler
    {
        protected const string Blur = "BlurBaseUI";
        public event Action<CustomWindow, bool> BlurBaseWindow;
        public static mStack<CustomWindow> PopWindows=new ();

        [SerializeField]
        UnityEvent m_ShowPage;

        /// <summary>
        /// Invoke this when window enable to play animation.
        /// </summary>
        public UnityEvent ShowPage { get => m_ShowPage; set => m_ShowPage = value; }

        [SerializeField]
        UnityEvent m_ClosePage;
        /// <summary>
        /// Invoke event to play animation before disable window.
        /// </summary>
        public UnityEvent ClosePage { get => m_ClosePage; set => m_ClosePage = value; }


        #region Routine

        protected virtual void OnEnable()
        {
            Debug.Log($"UI window enable {this}.");
            ShowPage?.Invoke();
            BlurBaseWindow?.Invoke(this, true);
            PopWindows.Push(this);
        }

        protected virtual void OnDisable()
        {
            Debug.Log($"UI window disable {this}.");
            ClosePage?.Invoke();
            BlurBaseWindow?.Invoke(this, false);
            PopWindows.Remove(this);
        }

        #endregion


        [SerializeField, Required]
        GameObject m_RootObject;
        public GameObject RootObject { get => m_RootObject; set => m_RootObject = value; }

        public void OnSelect(BaseEventData eventData)
        {
            PopWindows.SetAsNew(this);
        }



        /// <summary>
        /// Close window.
        /// </summary>
        public virtual void ExitPage()
        {
            enabled = false;
            // cancel blur
        }

    }
}