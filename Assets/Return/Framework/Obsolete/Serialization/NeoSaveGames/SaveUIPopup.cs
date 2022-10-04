using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoSaveGames
{
    [HelpURL("https://docs.neofps.com/manual/savegames-index.html")]
    public class SaveUIPopup : MonoBehaviour
    {
        [SerializeField, Tooltip("When to show the popup")]
        private OnEvent m_OnEvent = OnEvent.SaveInProgress;

        [SerializeField, Tooltip("Show the popup for a quick save")]
        private bool m_ShowQuickSaves = true;

        [SerializeField, Tooltip("Show the popup for an auto save")]
        private bool m_ShowAutoSaves = true;

        [SerializeField, Tooltip("Show the popup for a manual save")]
        private bool m_ShowManualSaves = false;

        [SerializeField, Tooltip("The minimum amount of time the popup should stay visible for")]
        private float m_MinDuration = 3f;

        private float m_Timer = 0f;
        private bool m_Hidden = false;

        protected CanvasGroup canvasGroup
        {
            get;
            private set;
        }
        
        public enum OnEvent
        {
            SaveInProgress,
            SaveFailed
        }

        protected void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();

            if (m_OnEvent == OnEvent.SaveInProgress)
                SaveGameManager.onSaveInProgess += EventHandler;
            else
                SaveGameManager.onSaveFailed += EventHandler;

            gameObject.SetActive(false);
        }

        protected void OnDestroy()
        {
            if (m_OnEvent == OnEvent.SaveInProgress)
                SaveGameManager.onSaveInProgess -= EventHandler;
            else
                SaveGameManager.onSaveFailed -= EventHandler;
        }

        protected virtual void Update()
        {
            if (!m_Hidden)
            {
                m_Timer += Time.unscaledDeltaTime;
                if (m_Timer > m_MinDuration && !SaveGameManager.inProgress)
                {
                    m_Hidden = true;
                    Hide();
                }
            }
        }

        void EventHandler(SaveGameType saveType)
        {
            switch (saveType)
            {
                case SaveGameType.Quicksave:
                    if (m_ShowQuickSaves)
                    {
                        m_Hidden = false;
                        m_Timer = 0f;
                        Show();
                    }
                    break;
                case SaveGameType.Autosave:
                    if (m_ShowAutoSaves)
                    {
                        m_Hidden = false;
                        m_Timer = 0f;
                        Show();
                    }
                    break;
                case SaveGameType.Manual:
                    if (m_ShowManualSaves)
                    {
                        m_Hidden = false;
                        m_Timer = 0f;
                        Show();
                    }
                    break;
            }
        }

        protected virtual void Show()
        {
            gameObject.SetActive(true);
            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
        }

        protected virtual void Hide()
        {
            gameObject.SetActive(false);
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }
    }
}