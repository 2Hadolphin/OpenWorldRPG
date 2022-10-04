#if UNITY_STANDALONE 
// Should other platforms use Json text files saved to disk?
#define SETTINGS_USES_JSON
#endif

using Return.Database;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Return.Items.Firearms
{
    /// <summary>
    /// **Crosshair color **auto reload 
    /// </summary>
    public class FirearmsSettings : SettingsContext<FirearmsSettings>
    {
        protected override string contextName { get { return "Gameplay"; } }

        public override string displayTitle { get { return "Firearms Settings"; } }

        public override string tocName { get { return "Gameplay Settings"; } }

        public override string tocID { get { return "settings_gameplay"; } }

        [SerializeField, Tooltip("The default player name.")]
        private string m_PlayerName = "Player";

        [SerializeField, Tooltip("The default player colour.")]
        private int m_PlayerColourCode = 0;

        [SerializeField, Tooltip("The default crosshair colour.")]
        private Color m_CrosshairColor = Color.white;

        [SerializeField, Range(0f, 1f), Tooltip("The ratio of head vs item bob. Head bob looks more natural when close to scenery, but some people can find it uncomfortable.")]
        private float m_HeadBob = 0.75f;

        [SerializeField, Tooltip("Should the player character automatically switch to a better weapon when picking it up.")]
        private bool m_AutoSwitchWeapons = true;

        [SerializeField, Tooltip("Should the selected firearm be reloaded automatically if hitting fire when empty.")]
        private bool m_AutoReload = true;

        public event UnityAction<Color> onCrosshairColorChanged;
        public event UnityAction<float> onHeadBobChanged;


#if SETTINGS_USES_JSON

        public string playerName
        {
            get { return m_PlayerName; }
            set { SetValue(ref m_PlayerName, value); }
        }

        public int playerColourCode
        {
            get { return m_PlayerColourCode; }
            set { SetValue(ref m_PlayerColourCode, value); }
        }

        public Color crosshairColor
        {
            get { return m_CrosshairColor; }
            set
            {
                SetValue(ref m_CrosshairColor, value);
                if (onCrosshairColorChanged != null)
                    onCrosshairColorChanged(value);
            }
        }

        public float headBob
        {
            get { return m_HeadBob; }
            set
            {
                SetValue(ref m_HeadBob, value);
                if (onHeadBobChanged != null)
                    onHeadBobChanged(m_HeadBob);
            }
        }

        public bool autoSwitchWeapons
        {
            get { return m_AutoSwitchWeapons; }
            set { SetValue(ref m_AutoSwitchWeapons, value); }
        }

        public bool autoReload
        {
            get { return m_AutoReload; }
            set { SetValue(ref m_AutoReload, value); }
        }

#else

		public string playerName
		{
			get { return GetString ("ps.playerName", m_PlayerName); }
			set { SetString ("ps.playerName", value); }
		}

		public int playerColourCode
		{
			get { return GetInt ("ps.playerColourCode", m_PlayerColourCode); }
			set { SetInt ("ps.playerColourCode", value); }
		}

		public Color crosshairColor
		{
			get { return GetColor ("ps.crosshairColor", m_CrosshairColor); }
			set
			{
				SetColor ("ps.crosshairColor", value);
				if (onCrosshairColorChanged != null)
					onCrosshairColorChanged (value);
			}
		}

        public float headBob
        {
			get { return GetFloat ("ps.headBob", m_HeadBob); }
			set
            {
                value = Mathf.Clamp01(value);
                SetFloat ("ps.playerColourCode", value);
                if (onHeadBobChanged != null)
                    onHeadBobChanged(value);
            }
        }

		public bool autoSwitchWeapons
		{
			get { return GetBool ("ps.autoSwitchWeapons", m_AutoSwitchWeapons); }
			set { SetBool ("ps.autoSwitchWeapons", value); }
		}
		
		public bool autoReload
		{
			get { return GetBool ("ps.autoReload", m_AutoReload); }
			set { SetBool ("ps.autoReload", value); }
		}

#endif
    }
}

