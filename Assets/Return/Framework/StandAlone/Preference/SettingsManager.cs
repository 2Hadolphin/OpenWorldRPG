using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using System;
using Return.Preference.GamePlay;
//using Zenject;

namespace Return.Preference
{
    [Obsolete]
    public class SettingsManager : SingletonSOManager<SettingsManager>,IStart//,IInitializable
    {
        [Obsolete]
        public static UEvent<SettingsManager> GamePlayUpdate;

        void IStart.Initialize() => Initialize();

        public override void Initialize()
        {
            //Debug.Log(this);
            GamePlay = ScriptableObject.CreateInstance<GamePlaySetting>();


        }


        //public override void LoadInitilization_BeforeSceneLoad()
        //{

        //    //GamePlayUpdate = new UEvent<SettingManager>(Instance);
        //}

        /// <summary>
        /// Provides the behaviour settings of several actions, such Running, Aiming and mouse axes.
        /// </summary>
        [SerializeField]
        [Required]
        [Tooltip("Provides the behaviour settings of several actions, such Running, Aiming and mouse axes.")]
        protected GamePlaySetting m_GamePlay;
        public GamePlaySetting GamePlay { get=>m_GamePlay; private set => m_GamePlay = value; }



        public AssistPreference Preference { get; private set; }


        public class AssistPreference
        {
            #region Interact
            public bool AutoStore;


            #endregion
        }


        protected override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// Provides all buttons and axes used by the character.
        /// </summary>
        [SerializeField]
        [Required]
        [Tooltip("Provides all buttons and axes used by the character.")]
        private InputActionAsset m_InputBindings;

        #region PROPERTIES

        #region Habit

        public Side PrimeHand = Side.Right | Side.Left;



        #endregion


        /// <summary>
        /// Is the character dead?
        /// </summary>
        public bool IsDead
        {
            get;
            set;
        }


        /// <summary>
        /// Returns the overall mouse sensitivity.
        /// </summary>
        public float OverallMouseSensitivity => GamePlay.OverallMouseSensitivity;

        /// <summary>
        /// Is the horizontal mouse Input reversed?
        /// </summary>
        public bool InvertHorizontalAxis => GamePlay.InvertHorizontalAxis;

        /// <summary>
        /// Is the vertical mouse Input reversed?
        /// </summary>
        public bool InvertVerticalAxis => GamePlay.InvertVerticalAxis;

        /// <summary>
        /// Returns the main camera field of view used by this character.
        /// </summary>
        public float FieldOfView => GamePlay.FieldOfView;

        /// <summary>
        /// Find an InputActionMap by its name in the InputActionAsset.
        /// </summary>
        /// <param name="mapName"></param>
        //public InputActionMap GetActionMap(string mapName)
        //{
        //    return m_InputBindings.FindActionMap(mapName);
        //}

        #endregion

        public void SetFOV(float fov)
        {
            GamePlay.FieldOfView = fov;
        }

        public void ChangeAimMode()
        {
            switch (GamePlay.AimStyle)
            {
                case ActionMode.Hold:
                    GamePlay.AimStyle = ActionMode.Toggle;
                    return;
                case ActionMode.Toggle:
                    GamePlay.AimStyle = ActionMode.Hold;
                    break;
            }
        }

        public void ChangeCrouchMode()
        {
            switch (GamePlay.CrouchStyle)
            {
                case ActionMode.Hold:
                    GamePlay.CrouchStyle = ActionMode.Toggle;
                    return;
                case ActionMode.Toggle:
                    GamePlay.CrouchStyle = ActionMode.Hold;
                    break;
            }
        }

        public void ChangeSprintMode()
        {
            switch (GamePlay.SprintStyle)
            {
                case ActionMode.Hold:
                    GamePlay.SprintStyle = ActionMode.Toggle;
                    return;
                case ActionMode.Toggle:
                    GamePlay.SprintStyle = ActionMode.Hold;
                    break;
            }
        }

        public void ChangeLeanMode()
        {
            switch (GamePlay.LeanStyle)
            {
                case ActionMode.Hold:
                    GamePlay.LeanStyle = ActionMode.Toggle;
                    return;
                case ActionMode.Toggle:
                    GamePlay.LeanStyle = ActionMode.Hold;
                    break;
            }
        }

        public void SetMouseSensitivity(float sensitivity)
        {
            GamePlay.OverallMouseSensitivity = sensitivity;
        }

        public void SetInvertHorizontalAxis(bool invert)
        {
            GamePlay.InvertHorizontalAxis = invert;
        }

        public void SetInvertVerticalAxis(bool invert)
        {
            GamePlay.InvertVerticalAxis = invert;
        }

    }
}