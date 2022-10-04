#if UNITY_STANDALONE // Should other platforms use Json text files saved to disk?
#define SETTINGS_USES_JSON
#endif

using System;
using UnityEngine;

#if SETTINGS_USES_JSON
using System.IO;
#else
using System.Text;
#endif

namespace Return.Database
{
    /// <summary>
    /// Load file from resources path.
    /// </summary>
    public abstract class SettingsContext<T> : ContextBase where T : SettingsContext<T>
    {
        protected static T instance
        {
            get;
            private set;
        }

        public static T GetInstance(string filename = null)
        {
            if (instance == null)
            {
                if (string.IsNullOrEmpty(filename))
                    filename = nameof(T);

                // Load the settings from resources
                var loaded = Resources.Load<T>(filename);

                if (loaded == null)
                {
                    // Not found - Create new
                    instance = CreateInstance<T>();
                }
                else
                {
                    // Found - Duplicate to prevent changing settings in game overwriting in editor
                    instance = Instantiate(loaded);
                }
            }
            return instance;
        }

        protected override bool CheckIfCurrent()
        {
            return instance == this;
        }
    }
}

