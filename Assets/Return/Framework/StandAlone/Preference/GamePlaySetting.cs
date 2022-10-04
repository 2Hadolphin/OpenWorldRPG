using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using Return.Database;

namespace Return.Preference.GamePlay
{

    /// <summary>
    /// Gameplay Settings Asset is a data container for the behaviour settings of some player actions.
    /// </summary>
    //[CreateAssetMenu(menuName = "MyData/Gameplay Settings", fileName = "Gameplay Control Settings", order = 201)]
    [Serializable]
	public class GamePlaySetting : SettingsContext<GamePlaySetting>
    {
		protected override string contextName { get { return "Gameplay"; } }

		public override string displayTitle { get { return "Gameplay Settings"; } }

		public override string tocName { get { return "Gameplay Settings"; } }

		public override string tocID { get { return "settings_gameplay"; } }

		protected override bool CheckIfCurrent()
		{
			return true;
		}

		public static UEvent<GamePlaySetting> OnUpdate;

        protected void OnEnable()
        {
			if(OnUpdate==null)
				OnUpdate = new UEvent<GamePlaySetting>(this);
        }


        #region Assit

        public bool AutoVault = false;

		#endregion

		#region Preference
		[Title("Preference")]

		/// <summary>
		/// Defines the Aim Down Sights behavior according to Input mode.
		/// </summary>
		[SerializeField]
        [Tooltip("Defines the Aim Down Sights behavior according to Input mode.")]
        protected ActionMode m_AimStyle = ActionMode.Toggle;

        /// <summary>
        /// Defines the Crouch behavior according to Input mode.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines the Crouch behavior according to Input mode.")]
        protected ActionMode m_CrouchStyle = ActionMode.Toggle;

        /// <summary>
        /// Defines the Sprint behavior according to Input mode.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines the Sprint behavior according to Input mode.")]
        protected ActionMode m_SprintStyle = ActionMode.Toggle;

        /// <summary>
        /// Defines the Lean behavior according to Input mode.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines the Lean behavior according to Input mode.")]
        protected ActionMode m_LeanStyle = ActionMode.Toggle;

		#region Port

		/// <summary>
		/// Defines the crouch behaviour according to the player's Input.
		/// </summary>
		public ActionMode CrouchStyle
		{
			get => m_CrouchStyle;
			set => m_CrouchStyle = value;
		}

		/// <summary>
		/// Defines the aiming behaviour according to the player's Input.
		/// </summary>
		public ActionMode AimStyle
		{
			get => m_AimStyle;
			set => m_AimStyle = value;
		}

		/// <summary>
		/// Defines the running behaviour according to the player's Input.
		/// </summary>
		public ActionMode SprintStyle
		{
			get => m_SprintStyle;
			set => m_SprintStyle = value;
		}

		/// <summary>
		/// Defines the leaning behaviour according to the player's Input.
		/// </summary>
		public ActionMode LeanStyle
		{
			get => m_LeanStyle;
			set => m_LeanStyle = value;
		}
		#endregion

		#endregion

		#region Input Control

		[Title("Mouse")]

		/// <summary>
		/// Defines whether the camera・s horizontal movement must be opposite to the mouse movement.
		/// </summary>
		[SerializeField]
		[Tooltip("Defines whether the camera・s horizontal movement must be opposite to the mouse movement.")]
		protected bool m_InvertHorizontalAxis;

		/// <summary>
		/// Defines whether the camera・s vertical movement must be opposite to the mouse movement.
		/// </summary>
		[SerializeField]
		[Tooltip("Defines whether the camera・s vertical movement must be opposite to the mouse movement.")]
		protected bool m_InvertVerticalAxis;


		/// <summary>
		/// Defines the overall mouse sensitivity.
		/// </summary>
		[SerializeField]
        [Range(0.1f, 10)]
        [Tooltip("Defines the overall mouse sensitivity.")]
        protected float m_OverallMouseSensitivity = 1;

		[SerializeField, Tooltip("The horizontal mouse look sensitivity.")]
		protected float m_MouseSensitivityH = 0.5f;

		[SerializeField, Tooltip("The vertical mouse look sensitivity.")]
		protected float m_MouseSensitivityV = 0.5f;




		[SerializeField, Tooltip("Mouse smoothing takes a weighted average of the mouse movement over time for a smoother effect.")]
		protected bool m_EnableMouseSmoothing = false;

		[SerializeField, Tooltip("The amount of mouse smoothing to add.")]
		protected float m_MouseSmoothing = 0.5f;

		[SerializeField, Tooltip("Mouse acceleration amplifies faster mouse movements.")]
		protected bool m_EnableMouseAcceleration = false;

		[SerializeField, Tooltip("The amount of mouse acceleration to add.")]
		protected float m_MouseAcceleration = 0.5f;



		#region Port

		/// <summary>
		/// Returns true if the mouse Input is reversed, false otherwise.
		/// </summary>
		public bool InvertHorizontalAxis
		{
			get => m_InvertHorizontalAxis;
			set
			{
				m_InvertHorizontalAxis = value;
				onMouseSettingsChanged?.Invoke();
			}
		}

		/// <summary>
		/// Returns true if the mouse Input is reversed, false otherwise.
		/// </summary>
		public bool InvertVerticalAxis
		{
			get => m_InvertVerticalAxis;
			set
			{
				m_InvertVerticalAxis = value;
				onMouseSettingsChanged?.Invoke();
			}
		}



		/// <summary>
		/// Defines the overall mouse sensitivity.
		/// </summary>
		public float OverallMouseSensitivity
		{
			get => m_OverallMouseSensitivity;
			set => m_OverallMouseSensitivity = Mathf.Clamp(value, 0.1f, 10);
		}

		/// <summary>
		/// Include invert mouse and global sensitivity
		/// </summary>
		public float horizontalMouseSensitivity
		{
			//get { return GetFloat("is.horizontalSensitivity", m_MouseSensitivityH); }
			get => m_MouseSensitivityH * m_OverallMouseSensitivity*m_InvertHorizontalAxis.ToInvertDirection();
			set
			{
				m_MouseSensitivityH = value;
                //SetFloat("is.horizontalSensitivity", value);
                onMouseSettingsChanged?.Invoke();
            }
		}

		/// <summary>
		/// Include invert mouse and global sensitivity
		/// </summary>
		public float verticalMouseSensitivity
		{
			//get { return GetFloat("is.verticalSensitivity", m_MouseSensitivityV); }
			get => -m_MouseSensitivityV * m_OverallMouseSensitivity*m_InvertVerticalAxis.ToInvertDirection();
			set
			{
				m_MouseSensitivityV = value;
                //SetFloat("is.verticalSensitivity", value);
                onMouseSettingsChanged?.Invoke();
            }
		}

		//public bool invertMouse
		//{
		//	get { return GetBool("is.invertMouse", m_InvertMouse); }
		//	set
		//	{
		//		SetBool("is.invertMouse", value);
		//		if (onMouseSettingsChanged != null)
		//			onMouseSettingsChanged();
		//	}
		//}



		public bool enableMouseSmoothing
		{
			//get { return GetBool("is.enableMouseSmoothing", m_EnableMouseSmoothing); }
			get => m_EnableMouseSmoothing;
			set
			{
				m_EnableMouseSmoothing = value;
				//SetBool("is.enableMouseSmoothing", value);
				onMouseSettingsChanged?.Invoke();
			}
		}

		public float mouseSmoothing
		{
			//get { return GetFloat("is.mouseSmoothing", m_MouseSmoothing); }
			get => m_MouseSmoothing;
			set
			{
				m_MouseSmoothing = value;
				//SetFloat("is.mouseSmoothing", value);
				onMouseSettingsChanged?.Invoke();
			}
		}

		public bool enableMouseAcceleration
		{
			//get { return GetBool("is.enableMouseAcceleration", m_EnableMouseAcceleration); }
			get => m_EnableMouseAcceleration;
			set
			{
				m_EnableMouseAcceleration = value;
				//SetBool("is.enableMouseAcceleration", value);
				onMouseSettingsChanged?.Invoke();
			}
		}

		public float mouseAcceleration
		{
			//get { return GetFloat("is.mouseAcceleration", m_MouseAcceleration); }
			get => m_MouseAcceleration;
			set
			{
				m_MouseAcceleration = value;
				//SetFloat("is.mouseAcceleration", value);
				onMouseSettingsChanged?.Invoke();
			}
		}
		#endregion

		#endregion


		#region View


		[Title("Visual")]

		/// <summary>
		/// Defines the camera FOV in hip-fire mode.
		/// </summary>
		[SerializeField]
        [Range(50, 90)]
        [Tooltip("Defines the camera FOV in hip-fire mode.")]
        protected float m_FieldOfView = 75;

        [SerializeField]
        [Range(50, 90)]
        [Tooltip("Defines the camera FOV in hip-fire mode.")]
		protected float FirstPersonItemFov = 55;

		#region PROPERTIES


		/// <summary>
		/// Returns the target field of view used by the character main camera.
		/// </summary>
		public float FieldOfView
		{
			get => m_FieldOfView;
			set
			{
				m_FieldOfView = Mathf.Clamp(value, 50, 90);
				OnUpdate?.Invoke(this, this);
			}
		}

		#endregion

		#endregion


		protected void OnValidate()
		{
			m_MouseSensitivityH = Mathf.Clamp(m_MouseSensitivityH, 0.1f, 5f);
			m_MouseSensitivityV = Mathf.Clamp(m_MouseSensitivityV, 0.1f, 5f);
			m_MouseAcceleration = Mathf.Clamp01(m_MouseAcceleration);
			m_MouseSmoothing = Mathf.Clamp01(m_MouseSmoothing);
		}


        public event UnityAction onMouseSettingsChanged;

#if SETTINGS_USES_JSON

		public float horizontalMouseSensitivity
		{
			get { return m_MouseSensitivityH; }
			set
			{
				SetValue (ref m_MouseSensitivityH, Mathf.Clamp(value, 0.1f, 5f));
				if (onMouseSettingsChanged != null)
					onMouseSettingsChanged ();
			}
		}

		public float verticalMouseSensitivity
		{
			get { return m_MouseSensitivityV; }
			set
			{
				SetValue (ref m_MouseSensitivityV, Mathf.Clamp(value, 0.1f, 5f));
				if (onMouseSettingsChanged != null)
					onMouseSettingsChanged ();
			}
		}

		public bool invertMouse
		{
			get { return m_InvertMouse; }
			set
			{
				SetValue (ref m_InvertMouse, value);
				if (onMouseSettingsChanged != null)
					onMouseSettingsChanged ();
			}
		}

		public bool enableMouseSmoothing
		{
			get { return m_EnableMouseSmoothing; }
			set
			{
				SetValue (ref m_EnableMouseSmoothing, value);
				if (onMouseSettingsChanged != null)
					onMouseSettingsChanged ();
			}
		}

		public float mouseSmoothing
		{
			get { return m_MouseSmoothing; }
			set
			{
				SetValue (ref m_MouseSmoothing, Mathf.Clamp01(value));
				if (onMouseSettingsChanged != null)
					onMouseSettingsChanged ();
			}
		}

		public bool enableMouseAcceleration
		{
			get { return m_EnableMouseAcceleration; }
			set
			{
				SetValue (ref m_EnableMouseAcceleration, value);
				if (onMouseSettingsChanged != null)
					onMouseSettingsChanged ();
			}
		}

		public float mouseAcceleration
		{
			get { return m_MouseAcceleration; }
			set
			{
				SetValue (ref m_MouseAcceleration, Mathf.Clamp01(value));
				if (onMouseSettingsChanged != null)
					onMouseSettingsChanged ();
			}
		}

#else




#endif
	}
}