using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoSaveGames
{
    [HelpURL("https://docs.neofps.com/manual/savegamesref-mb-autosavebehaviour.html")]
    public class AutoSaveBehaviour : MonoBehaviour
    {
        [SerializeField, Tooltip("Can the autosave be triggered multiple times by this behaviour.")]
        private bool m_OneShot = true;

        [SerializeField, Tooltip("The time in seconds before the autosave can be triggered again.")]
        private float m_Cooldown = 10f;

        [SerializeField, Range(0, 10), Tooltip("If the save fails, the behaviour will try every second this manny times.")]
        private int m_RetryAttempts = 0;

        private bool m_Blocked = false;
        private Coroutine m_RetryCoroutine = null;

        protected void OnValidate()
        {
            m_Cooldown = Mathf.Clamp(m_Cooldown, 1f, 300f);
        }

        public void AutoSave()
        {
            // Check if blocked (cooldown or one-shot)
            if (m_Blocked)
                return;

            // Cancel retry attempts
            if (m_RetryCoroutine != null)
            {
                StopCoroutine(m_RetryCoroutine);
                m_RetryCoroutine = null;
            }

            // Save the game
            var saved = AutoSaveInternal();

            // Retry if failed
            if (!saved && m_RetryAttempts > 0)
                m_RetryCoroutine = StartCoroutine(RetrySave());
        }

        public bool AutoSaveInternal()
        {
            if (SaveGameManager.AutoSave())
            {
                if (m_OneShot)
                    m_Blocked = true;
                else
                    StartCoroutine(Cooldown());

                return true;
            }
            else
                return false;
        }

        IEnumerator RetrySave()
        {
            float timer = 0f;
            int attempts = 0;

            while(attempts < m_RetryAttempts)
            {
                yield return null;

                timer += Time.unscaledDeltaTime;
                if (timer > 1f)
                {
                    timer -= 1f;
                    ++attempts;

                    // Attempt to save again
                    if (AutoSaveInternal())
                        break;
                }
            }

            m_RetryCoroutine = null;
        }

        IEnumerator Cooldown()
        {
            m_Blocked = true;

            float timer = m_Cooldown;
            while (timer > 0f)
            {
                yield return null;
                timer -= Time.unscaledDeltaTime;
            }

            m_Blocked = false;
        }
    }
}