#define SETTINGS_USES_JSON

using System;
using UnityEngine;
#if SETTINGS_USES_JSON
using System.IO;
#else
using System.Text;
#endif




namespace Return.Database
{
    public abstract class ContextBase : PresetDatabase
    {
        protected abstract string contextName
        {
            get;
        }

        public abstract string displayTitle
        {
            get;
        }

        [Obsolete]
        public abstract string tocName
        {
            get;
        }

        [Obsolete]
        public abstract string tocID
        {
            get;
        }

        public bool dirty
        {
            get;
            protected set;
        }

        protected abstract bool CheckIfCurrent();

#if SETTINGS_USES_JSON

        private string m_Filepath = string.Empty;
        protected string filepath
        {
            get
            {
                if (m_Filepath == string.Empty)
                {
#if UNITY_EDITOR
                    m_Filepath = Application.dataPath + "/../" + contextName + ".settings";
#else
					m_Filepath = Path.Combine (Application.dataPath, contextName + ".settings");
#endif
                }
                return m_Filepath;
            }
        }

        public void DeleteSaveFile()
        {
            if (File.Exists(filepath))
                File.Delete(filepath);
        }

        protected void SetValue<V>(ref V target, V to)
        {
            target = to;
            dirty = true;
        }

        public virtual void Load()
        {
            if (File.Exists(filepath))
            {
                string json = File.ReadAllText(filepath);
                JsonUtility.FromJsonOverwrite(json, this);
            }
            else
            {
                // Save out with default settings
                File.WriteAllText(filepath, JsonUtility.ToJson(this, true));
            }
            dirty = false;
            OnLoad();
        }

        public virtual void Save()
        {
            if (dirty)
            {
                File.WriteAllText(filepath, JsonUtility.ToJson(this, true));
                dirty = false;
            }
            OnSave();
        }

        public virtual void OnLoad() { }
        public virtual void OnSave() { }

#else

        // Need a better solution for arrays (a playerprefs collection that handles indexing)

        private StringBuilder m_StringBuilder = new StringBuilder();

        protected int GetInt(string key, int defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        protected float GetFloat(string key, float defaultValue)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        protected bool GetBool(string key, bool defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) != 0;
        }

        protected string GetString(string key, string defaultValue)
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        protected Color GetColor(string key, Color defaultValue)
        {
            Color result = defaultValue;

            // Set base key
            int length = key.Length;
            m_StringBuilder.Append(key);

            // Build "R" key and check if it exists
            m_StringBuilder.Append(".r");
            string rKey = m_StringBuilder.ToString();
            if (PlayerPrefs.HasKey(rKey))
            {
                // Get R
                result.r = PlayerPrefs.GetFloat(rKey, result.r);

                // Get G
                m_StringBuilder.Remove(length, 2);
                m_StringBuilder.Append(".g");
                result.g = PlayerPrefs.GetFloat(m_StringBuilder.ToString(), result.g);

                // Get B
                m_StringBuilder.Remove(length, 2);
                m_StringBuilder.Append(".b");
                result.b = PlayerPrefs.GetFloat(m_StringBuilder.ToString(), result.b);

                // Get A
                m_StringBuilder.Remove(length, 2);
                m_StringBuilder.Append(".a");
                result.a = PlayerPrefs.GetFloat(m_StringBuilder.ToString(), result.a);
            }

            // Reset the builder and return the result
            m_StringBuilder.Length = 0;
            return result;
        }

        protected Vector2 GetVector2(string key, Vector2 defaultValue)
        {
            Vector2 result = defaultValue;

            // Set base key
            int length = key.Length;
            m_StringBuilder.Append(key);

            // Build "R" key and check if it exists
            m_StringBuilder.Append(".x");
            string rKey = m_StringBuilder.ToString();
            if (PlayerPrefs.HasKey(rKey))
            {
                // Get X
                result.x = PlayerPrefs.GetFloat(rKey, result.x);

                // Get Y
                m_StringBuilder.Remove(length, 2);
                m_StringBuilder.Append(".y");
                result.y = PlayerPrefs.GetFloat(m_StringBuilder.ToString(), result.y);
            }

            // Reset the builder and return the result
            m_StringBuilder.Length = 0;
            return result;
        }

        protected Vector3 GetVector3(string key, Vector3 defaultValue)
        {
            Vector3 result = defaultValue;

            // Set base key
            int length = key.Length;
            m_StringBuilder.Append(key);

            // Build "R" key and check if it exists
            m_StringBuilder.Append(".x");
            string rKey = m_StringBuilder.ToString();
            if (PlayerPrefs.HasKey(rKey))
            {
                // Get X
                result.x = PlayerPrefs.GetFloat(rKey, result.x);

                // Get Y
                m_StringBuilder.Remove(length, 2);
                m_StringBuilder.Append(".y");
                result.y = PlayerPrefs.GetFloat(m_StringBuilder.ToString(), result.y);

                // Get Z
                m_StringBuilder.Remove(length, 2);
                m_StringBuilder.Append(".z");
                result.z = PlayerPrefs.GetFloat(m_StringBuilder.ToString(), result.z);
            }

            // Reset the builder and return the result
            m_StringBuilder.Length = 0;
            return result;
        }

        protected void SetInt(string key, int to)
        {
            PlayerPrefs.SetInt(key, to);
        }

        protected void SetFloat(string key, float to)
        {
            PlayerPrefs.SetFloat(key, to);
        }

        protected void SetBool(string key, bool to)
        {
            PlayerPrefs.SetInt(key, (to) ? 1 : 0);
        }

        protected void SetString(string key, string to)
        {
            PlayerPrefs.SetString(key, to);
        }

        protected void SetColor(string key, Color to)
        {
            // Set base key
            int length = key.Length;
            m_StringBuilder.Append(key);

            // Set R
            m_StringBuilder.Append(".r");
            PlayerPrefs.SetFloat(m_StringBuilder.ToString(), to.r);

            // Set G
            m_StringBuilder.Remove(length, 2);
            m_StringBuilder.Append(".g");
            PlayerPrefs.SetFloat(m_StringBuilder.ToString(), to.g);

            // Set B
            m_StringBuilder.Remove(length, 2);
            m_StringBuilder.Append(".b");
            PlayerPrefs.SetFloat(m_StringBuilder.ToString(), to.b);

            // Set A
            m_StringBuilder.Remove(length, 2);
            m_StringBuilder.Append(".a");
            PlayerPrefs.SetFloat(m_StringBuilder.ToString(), to.a);

            // Reset the builder and return the result
            m_StringBuilder.Length = 0;
        }

        protected void SetVector2(string key, Vector2 to)
        {
            // Set base key
            int length = key.Length;
            m_StringBuilder.Append(key);

            // Set X
            m_StringBuilder.Append(".x");
            PlayerPrefs.SetFloat(m_StringBuilder.ToString(), to.x);

            // Set Y
            m_StringBuilder.Remove(length, 2);
            m_StringBuilder.Append(".y");
            PlayerPrefs.SetFloat(m_StringBuilder.ToString(), to.y);

            // Reset the builder and return the result
            m_StringBuilder.Length = 0;
        }

        protected void SetVector3(string key, Vector3 to)
        {
            // Set base key
            int length = key.Length;
            m_StringBuilder.Append(key);

            // Set X
            m_StringBuilder.Append(".x");
            PlayerPrefs.SetFloat(m_StringBuilder.ToString(), to.x);

            // Set Y
            m_StringBuilder.Remove(length, 2);
            m_StringBuilder.Append(".y");
            PlayerPrefs.SetFloat(m_StringBuilder.ToString(), to.y);

            // Set Z
            m_StringBuilder.Remove(length, 2);
            m_StringBuilder.Append(".z");
            PlayerPrefs.SetFloat(m_StringBuilder.ToString(), to.z);

            // Reset the builder and return the result
            m_StringBuilder.Length = 0;
        }

        protected int[] GetIntArray(string key, int[] defaultValue)
        {
            m_StringBuilder.Append(key);
            m_StringBuilder.Append(".count");

            int length = PlayerPrefs.GetInt(m_StringBuilder.ToString(), -1);
            if (length == -1)
            {
                m_StringBuilder.Length = 0;
                return defaultValue;
            }
            else
            {
                int[] result = new int[length];

                for (int i = 0; i < length; ++i)
                {
                    m_StringBuilder.Length = key.Length + 1;
                    m_StringBuilder.Append(i);
                    result[i] = PlayerPrefs.GetInt(m_StringBuilder.ToString());
                }

                // reset stringbuilder and return result
                m_StringBuilder.Length = 0;
                return result;
            }
        }

        protected float[] GetFloatArray(string key, float[] defaultValue)
        {
            m_StringBuilder.Append(key);
            m_StringBuilder.Append(".count");

            int length = PlayerPrefs.GetInt(m_StringBuilder.ToString(), -1);
            if (length == -1)
            {
                m_StringBuilder.Length = 0;
                return defaultValue;
            }
            else
            {
                float[] result = new float[length];

                for (int i = 0; i < length; ++i)
                {
                    m_StringBuilder.Length = key.Length + 1;
                    m_StringBuilder.Append(i);
                    result[i] = PlayerPrefs.GetFloat(m_StringBuilder.ToString());
                }

                // reset stringbuilder and return result
                m_StringBuilder.Length = 0;
                return result;
            }
        }

        protected bool[] GetBoolArray(string key, bool[] defaultValue)
        {
            m_StringBuilder.Append(key);
            m_StringBuilder.Append(".count");

            int length = PlayerPrefs.GetInt(m_StringBuilder.ToString(), -1);
            if (length == -1)
            {
                m_StringBuilder.Length = 0;
                return defaultValue;
            }
            else
            {
                bool[] result = new bool[length];

                for (int i = 0; i < length; ++i)
                {
                    m_StringBuilder.Length = key.Length + 1;
                    m_StringBuilder.Append(i);
                    result[i] = PlayerPrefs.GetInt(m_StringBuilder.ToString()) != 0;
                }

                // reset stringbuilder and return result
                m_StringBuilder.Length = 0;
                return result;
            }
        }

        protected string[] GetStringArray(string key, string[] defaultValue)
        {
            m_StringBuilder.Append(key);
            m_StringBuilder.Append(".count");

            int length = PlayerPrefs.GetInt(m_StringBuilder.ToString(), -1);
            if (length == -1)
            {
                m_StringBuilder.Length = 0;
                return defaultValue;
            }
            else
            {
                string[] result = new string[length];

                for (int i = 0; i < length; ++i)
                {
                    m_StringBuilder.Length = key.Length + 1;
                    m_StringBuilder.Append(i);
                    result[i] = PlayerPrefs.GetString(m_StringBuilder.ToString());
                }

                // reset stringbuilder and return result
                m_StringBuilder.Length = 0;
                return result;
            }
        }

        protected Color[] GetColorArray(string key, Color[] defaultValue)
        {
            m_StringBuilder.Append(key);
            m_StringBuilder.Append(".count");

            int length = PlayerPrefs.GetInt(m_StringBuilder.ToString(), -1);
            if (length == -1)
            {
                m_StringBuilder.Length = 0;
                return defaultValue;
            }
            else
            {
                Color[] result = new Color[length];

                for (int i = 0; i < length; ++i)
                {
                    m_StringBuilder.Length = key.Length + 1;
                    m_StringBuilder.Append(i);

                    m_StringBuilder.Append(".r");
                    result[i].r = PlayerPrefs.GetFloat(m_StringBuilder.ToString());

                    m_StringBuilder.Length -= 2;
                    m_StringBuilder.Append(".g");
                    result[i].g = PlayerPrefs.GetFloat(m_StringBuilder.ToString());

                    m_StringBuilder.Length -= 2;
                    m_StringBuilder.Append(".b");
                    result[i].b = PlayerPrefs.GetFloat(m_StringBuilder.ToString());

                    m_StringBuilder.Length -= 2;
                    m_StringBuilder.Append(".a");
                    result[i].a = PlayerPrefs.GetFloat(m_StringBuilder.ToString());
                }

                // reset stringbuilder and return result
                m_StringBuilder.Length = 0;
                return result;
            }
        }

        protected Vector2[] GetVector2Array(string key, Vector2[] defaultValue)
        {
            m_StringBuilder.Append(key);
            m_StringBuilder.Append(".count");

            int length = PlayerPrefs.GetInt(m_StringBuilder.ToString(), -1);
            if (length == -1)
            {
                m_StringBuilder.Length = 0;
                return defaultValue;
            }
            else
            {
                Vector2[] result = new Vector2[length];

                for (int i = 0; i < length; ++i)
                {
                    m_StringBuilder.Length = key.Length + 1;
                    m_StringBuilder.Append(i);

                    m_StringBuilder.Append(".x");
                    result[i].x = PlayerPrefs.GetFloat(m_StringBuilder.ToString());

                    m_StringBuilder.Length -= 2;
                    m_StringBuilder.Append(".y");
                    result[i].y = PlayerPrefs.GetFloat(m_StringBuilder.ToString());
                }

                // reset stringbuilder and return result
                m_StringBuilder.Length = 0;
                return result;
            }
        }

        protected Vector3[] GetVector3Array(string key, Vector3[] defaultValue)
        {
            m_StringBuilder.Append(key);
            m_StringBuilder.Append(".count");

            int length = PlayerPrefs.GetInt(m_StringBuilder.ToString(), -1);
            if (length == -1)
            {
                m_StringBuilder.Length = 0;
                return defaultValue;
            }
            else
            {
                Vector3[] result = new Vector3[length];

                for (int i = 0; i < length; ++i)
                {
                    m_StringBuilder.Length = key.Length + 1;
                    m_StringBuilder.Append(i);

                    m_StringBuilder.Append(".x");
                    result[i].x = PlayerPrefs.GetFloat(m_StringBuilder.ToString());

                    m_StringBuilder.Length -= 2;
                    m_StringBuilder.Append(".y");
                    result[i].y = PlayerPrefs.GetFloat(m_StringBuilder.ToString());

                    m_StringBuilder.Length -= 2;
                    m_StringBuilder.Append(".z");
                    result[i].z = PlayerPrefs.GetFloat(m_StringBuilder.ToString());
                }

                // reset stringbuilder and return result
                m_StringBuilder.Length = 0;
                return result;
            }
        }

        protected void SetIntArray(string key, int[] to)
        {
            m_StringBuilder.Append(key);
            m_StringBuilder.Append(".count");

            string k = m_StringBuilder.ToString();
            int oldLength = PlayerPrefs.GetInt(m_StringBuilder.ToString(), -1);

            if (to == null)
            {
                if (oldLength != -1)
                {
                    PlayerPrefs.DeleteKey(k);

                    for (int i = 0; i < oldLength; ++i)
                    {
                        m_StringBuilder.Length = key.Length + 1;
                        m_StringBuilder.Append(i);
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                    }
                }
            }
            else
            {
                PlayerPrefs.SetInt(m_StringBuilder.ToString(), to.Length);

                for (int i = 0; i < Mathf.Max(to.Length, oldLength); ++i)
                {
                    m_StringBuilder.Length = key.Length + 1;
                    m_StringBuilder.Append(i);

                    if (i < to.Length)
                        PlayerPrefs.SetInt(m_StringBuilder.ToString(), to[i]);
                    else
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                }
            }

            // reset stringbuilder
            m_StringBuilder.Length = 0;
        }

        protected void SetFloatArray(string key, float[] to)
        {
            m_StringBuilder.Append(key);
            m_StringBuilder.Append(".count");

            string k = m_StringBuilder.ToString();
            int oldLength = PlayerPrefs.GetInt(m_StringBuilder.ToString(), -1);

            if (to == null)
            {
                if (oldLength != -1)
                {
                    PlayerPrefs.DeleteKey(k);

                    for (int i = 0; i < oldLength; ++i)
                    {
                        m_StringBuilder.Length = key.Length + 1;
                        m_StringBuilder.Append(i);
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                    }
                }
            }
            else
            {
                PlayerPrefs.SetInt(m_StringBuilder.ToString(), to.Length);

                for (int i = 0; i < Mathf.Max(to.Length, oldLength); ++i)
                {
                    m_StringBuilder.Length = key.Length + 1;
                    m_StringBuilder.Append(i);

                    if (i < to.Length)
                        PlayerPrefs.SetFloat(m_StringBuilder.ToString(), to[i]);
                    else
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                }
            }

            // reset stringbuilder
            m_StringBuilder.Length = 0;
        }

        protected void SetBoolArray(string key, bool[] to)
        {
            m_StringBuilder.Append(key);
            m_StringBuilder.Append(".count");

            string k = m_StringBuilder.ToString();
            int oldLength = PlayerPrefs.GetInt(m_StringBuilder.ToString(), -1);

            if (to == null)
            {
                if (oldLength != -1)
                {
                    PlayerPrefs.DeleteKey(k);

                    for (int i = 0; i < oldLength; ++i)
                    {
                        m_StringBuilder.Length = key.Length + 1;
                        m_StringBuilder.Append(i);
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                    }
                }
            }
            else
            {
                PlayerPrefs.SetInt(m_StringBuilder.ToString(), to.Length);

                for (int i = 0; i < Mathf.Max(to.Length, oldLength); ++i)
                {
                    m_StringBuilder.Length = key.Length + 1;
                    m_StringBuilder.Append(i);

                    if (i < to.Length)
                        PlayerPrefs.SetInt(m_StringBuilder.ToString(), (to[i]) ? 1 : 0);
                    else
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                }
            }

            // reset stringbuilder
            m_StringBuilder.Length = 0;
        }

        protected void SetStringArray(string key, string[] to)
        {
            m_StringBuilder.Append(key);
            m_StringBuilder.Append(".count");

            string k = m_StringBuilder.ToString();
            int oldLength = PlayerPrefs.GetInt(m_StringBuilder.ToString(), -1);

            if (to == null)
            {
                if (oldLength != -1)
                {
                    PlayerPrefs.DeleteKey(k);

                    for (int i = 0; i < oldLength; ++i)
                    {
                        m_StringBuilder.Length = key.Length + 1;
                        m_StringBuilder.Append(i);
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                    }
                }
            }
            else
            {
                PlayerPrefs.SetInt(m_StringBuilder.ToString(), to.Length);

                for (int i = 0; i < Mathf.Max(to.Length, oldLength); ++i)
                {
                    m_StringBuilder.Length = key.Length + 1;
                    m_StringBuilder.Append(i);

                    if (i < to.Length)
                        PlayerPrefs.SetString(m_StringBuilder.ToString(), to[i]);
                    else
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                }
            }

            // reset stringbuilder
            m_StringBuilder.Length = 0;
        }

        protected void SetColorArray(string key, Color[] to)
        {
            m_StringBuilder.Append(key);
            m_StringBuilder.Append(".count");

            string k = m_StringBuilder.ToString();
            int oldLength = PlayerPrefs.GetInt(m_StringBuilder.ToString(), -1);

            if (to == null)
            {
                if (oldLength != -1)
                {
                    PlayerPrefs.DeleteKey(k);

                    for (int i = 0; i < oldLength; ++i)
                    {
                        m_StringBuilder.Length = key.Length + 1;
                        m_StringBuilder.Append(i);

                        m_StringBuilder.Append(".r");
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                        m_StringBuilder.Length -= 2;
                        m_StringBuilder.Append(".g");
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                        m_StringBuilder.Length -= 2;
                        m_StringBuilder.Append(".b");
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                        m_StringBuilder.Length -= 2;
                        m_StringBuilder.Append(".a");
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                    }
                }
            }
            else
            {
                PlayerPrefs.SetInt(m_StringBuilder.ToString(), to.Length);

                for (int i = 0; i < Mathf.Max(to.Length, oldLength); ++i)
                {
                    m_StringBuilder.Length = key.Length + 1;
                    m_StringBuilder.Append(i);

                    if (i < to.Length)
                    {
                        m_StringBuilder.Append(".r");
                        PlayerPrefs.SetFloat(m_StringBuilder.ToString(), to[i].r);
                        m_StringBuilder.Length -= 2;
                        m_StringBuilder.Append(".g");
                        PlayerPrefs.SetFloat(m_StringBuilder.ToString(), to[i].g);
                        m_StringBuilder.Length -= 2;
                        m_StringBuilder.Append(".b");
                        PlayerPrefs.SetFloat(m_StringBuilder.ToString(), to[i].b);
                        m_StringBuilder.Length -= 2;
                        m_StringBuilder.Append(".a");
                        PlayerPrefs.SetFloat(m_StringBuilder.ToString(), to[i].a);

                    }
                    else
                    {
                        m_StringBuilder.Append(".r");
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                        m_StringBuilder.Length -= 2;
                        m_StringBuilder.Append(".g");
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                        m_StringBuilder.Length -= 2;
                        m_StringBuilder.Append(".b");
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                        m_StringBuilder.Length -= 2;
                        m_StringBuilder.Append(".a");
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                    }
                }
            }

            // reset stringbuilder
            m_StringBuilder.Length = 0;
        }

        protected void SetVector2Array(string key, Vector2[] to)
        {
            m_StringBuilder.Append(key);
            m_StringBuilder.Append(".count");

            string k = m_StringBuilder.ToString();
            int oldLength = PlayerPrefs.GetInt(m_StringBuilder.ToString(), -1);

            if (to == null)
            {
                if (oldLength != -1)
                {
                    PlayerPrefs.DeleteKey(k);

                    for (int i = 0; i < oldLength; ++i)
                    {
                        m_StringBuilder.Length = key.Length + 1;
                        m_StringBuilder.Append(i);

                        m_StringBuilder.Append(".x");
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                        m_StringBuilder.Length -= 2;
                        m_StringBuilder.Append(".y");
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                    }
                }
            }
            else
            {
                PlayerPrefs.SetInt(m_StringBuilder.ToString(), to.Length);

                for (int i = 0; i < Mathf.Max(to.Length, oldLength); ++i)
                {
                    m_StringBuilder.Length = key.Length + 1;
                    m_StringBuilder.Append(i);

                    if (i < to.Length)
                    {
                        m_StringBuilder.Append(".x");
                        PlayerPrefs.SetFloat(m_StringBuilder.ToString(), to[i].x);
                        m_StringBuilder.Length -= 2;
                        m_StringBuilder.Append(".y");
                        PlayerPrefs.SetFloat(m_StringBuilder.ToString(), to[i].y);

                    }
                    else
                    {
                        m_StringBuilder.Append(".x");
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                        m_StringBuilder.Length -= 2;
                        m_StringBuilder.Append(".y");
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                    }
                }
            }

            // reset stringbuilder
            m_StringBuilder.Length = 0;
        }

        protected void SetVector3Array(string key, Vector3[] to)
        {
            m_StringBuilder.Append(key);
            m_StringBuilder.Append(".count");

            string k = m_StringBuilder.ToString();
            int oldLength = PlayerPrefs.GetInt(m_StringBuilder.ToString(), -1);

            if (to == null)
            {
                if (oldLength != -1)
                {
                    PlayerPrefs.DeleteKey(k);

                    for (int i = 0; i < oldLength; ++i)
                    {
                        m_StringBuilder.Length = key.Length + 1;
                        m_StringBuilder.Append(i);

                        m_StringBuilder.Append(".x");
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                        m_StringBuilder.Length -= 2;
                        m_StringBuilder.Append(".y");
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                        m_StringBuilder.Length -= 2;
                        m_StringBuilder.Append(".z");
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                    }
                }
            }
            else
            {
                PlayerPrefs.SetInt(m_StringBuilder.ToString(), to.Length);

                for (int i = 0; i < Mathf.Max(to.Length, oldLength); ++i)
                {
                    m_StringBuilder.Length = key.Length + 1;
                    m_StringBuilder.Append(i);

                    if (i < to.Length)
                    {
                        m_StringBuilder.Append(".x");
                        PlayerPrefs.SetFloat(m_StringBuilder.ToString(), to[i].x);
                        m_StringBuilder.Length -= 2;
                        m_StringBuilder.Append(".y");
                        PlayerPrefs.SetFloat(m_StringBuilder.ToString(), to[i].y);
                        m_StringBuilder.Length -= 2;
                        m_StringBuilder.Append(".z");
                        PlayerPrefs.SetFloat(m_StringBuilder.ToString(), to[i].z);

                    }
                    else
                    {
                        m_StringBuilder.Append(".x");
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                        m_StringBuilder.Length -= 2;
                        m_StringBuilder.Append(".y");
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                        m_StringBuilder.Length -= 2;
                        m_StringBuilder.Append(".z");
                        PlayerPrefs.DeleteKey(m_StringBuilder.ToString());
                    }
                }
            }

            // reset stringbuilder
            m_StringBuilder.Length = 0;
        }

        public virtual void Load() { OnLoad(); }
        public virtual void Save() { PlayerPrefs.Save(); OnSave(); }

        public virtual void OnLoad() { }
        public virtual void OnSave() { }
        
        public void DeleteSaveFile()
        {
            // Stub to prevent build errors
        }

#endif
    }
}
