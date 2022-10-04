﻿using Return.Items.Firearms;
using System;
using UnityEngine;

namespace NeoFPS
{
    public class FpsSettings
	{
		//public static FpsAudioSettings audio { get; private set; }
		public static FirearmsSettings gameplay { get; private set; }
		//public static FpsGraphicsSettings graphics { get; private set; }
		//public static FpsInputSettings Input { get; private set; }
		//public static FpsKeyBindings keyBindings { get; private set; }
		//public static FpsGamepadSettings gamepad { get; private set; }

        private static GameObject s_RuntimeSettingsObject = null;
        public static GameObject runtimeSettingsObject
        {
            get
            {
                if (s_RuntimeSettingsObject == null)
                {
                    s_RuntimeSettingsObject = new GameObject("NeoFpsSettingsRuntime");
                    UnityEngine.Object.DontDestroyOnLoad(s_RuntimeSettingsObject);
                }
                return s_RuntimeSettingsObject;
            }
        }

		[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void RuntimeInitializeOnLoad ()
		{
			//audio = FpsAudioSettings.GetInstance ("FpsSettings_Audio");
			//gameplay = FirearmsSettings.GetInstance ("FpsSettings_Gameplay");
			//graphics = FpsGraphicsSettings.GetInstance ("FpsSettings_Graphics");
			//Input = FpsInputSettings.GetInstance ("FpsSettings_Input");
			//keyBindings = FpsKeyBindings.GetInstance("FpsSettings_KeyBindings");
			//gamepad = FpsGamepadSettings.GetInstance("FpsSettings_Gamepad");
		}

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void RuntimeInitializeOnLoadPost()
        {
            //audio.Load();
            //gameplay.Load();
            //graphics.Load();
            //Input.Load();
            //keyBindings.Load();
            //gamepad.Load();
        }

        public static void Save ()
		{
			//audio.Save ();
			//gameplay.Save ();
			//graphics.Save ();
			//Input.Save ();
			//keyBindings.Save ();
			//gamepad.Save ();
		}
	}
}

