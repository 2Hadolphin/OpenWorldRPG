#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using NeoSaveGames.Serialization;

namespace NeoSaveGames
{
    public class SaveGameInspector : EditorWindow
    {
        [MenuItem("Tools/NeoFPS/NeoSave File Inspector", priority = 20)]
        public static void ShowWindow()
        {
            var window = GetWindow<SaveGameInspector>();
            window.titleContent = new GUIContent("Save Inspector");
            //window.minSize = new Vector2(896, 660);
        }

        public static void ShowWindow(SavePathRoot root, string subFolder)
        {
            if (s_Settings == null)
                s_Settings = CreateInstance<Settings>();

            s_Settings.savePathRoot = root;
            s_Settings.saveSubFolder = subFolder;

            var window = GetWindow<SaveGameInspector>();
            window.titleContent = new GUIContent("Save Inspector");
            window.m_FolderState = FolderState.Unchecked;
        }

        private static Settings s_Settings = null;
        private static Dictionary<int, string> m_KeyMap = new Dictionary<int, string>();

        private static BinaryDeserializer m_Reader = null;
        private List<SerializationContext> m_ContextStack = new List<SerializationContext>();
        private SerializedObject m_SerializedSettings = null;
        private FolderState m_FolderState = FolderState.Unchecked;
        private string m_CurrentState = string.Empty;
        private Vector2 m_ScrollPosition = Vector2.zero;
        private int m_ExpandedProperty = -1;

        enum FolderState
        {
            Unchecked,
            Invalid,
            ValidNoSaves,
            ValidWithSaves
        }

        [Serializable]
        class Settings : ScriptableObject
        {
            public bool expandLocationSettings = false;
            public SavePathRoot savePathRoot = SavePathRoot.PersistantDataPath;
            public string saveSubFolder = "SaveFiles";
        }

        [Serializable]
        class DictionaryExport
        {
            public KeyValuePairExport[] entries = null;

            public DictionaryExport(Dictionary<int, string> dictionary)
            {
                entries = new KeyValuePairExport[dictionary.Count];
                int i = 0;
                foreach (var kvp in dictionary)
                {
                    entries[i] = new KeyValuePairExport(kvp.Key, kvp.Value);
                    ++i;
                }
            }

            public void AddToDictionary(Dictionary<int, string> dictionary)
            {
                foreach (var kvp in entries)
                {
                    if (!dictionary.ContainsKey(kvp.hash))
                        dictionary.Add(kvp.hash, kvp.value);
                }
            }
        }

        [Serializable]
        struct KeyValuePairExport
        {
            public int hash;
            public string value;

            public KeyValuePairExport(int h, string v)
            {
                hash = h;
                value = v;
            }
        }

        private SerializationContext currentContextType
        {
            get
            {
                if (m_ContextStack.Count > 0)
                    return m_ContextStack[m_ContextStack.Count - 1];
                else
                    return SerializationContext.Root;
            }
        }

        private SerializationContext parentContextType
        {
            get
            {
                if (m_ContextStack.Count > 1)
                    return m_ContextStack[m_ContextStack.Count - 2];
                else
                    return SerializationContext.Root;
            }
        }

        private SerializedObject settings
        {
            get { return m_SerializedSettings; }
        }

        protected void Awake()
        {
            m_Reader = new BinaryDeserializer();
            m_ScrollPosition = Vector2.zero;
            AddDefaultKeyMaps();
        }

        protected void OnDestroy()
        {
            // Stop serializing old
            if (m_Reader != null && m_Reader.isDeserializing)
            {
                while (m_Reader.currentContext.contextType != SerializationContext.Root)
                    m_Reader.PopContext(m_Reader.currentContext.contextType, -1);
                m_Reader.EndDeserialization();
                m_ContextStack.Clear();
                m_ExpandedProperty = -1;
            }
            m_Reader = null;
        }

        protected void OnEnable()
        {
            if (s_Settings == null)
                s_Settings = CreateInstance<Settings>();
            m_SerializedSettings = new SerializedObject(s_Settings);
            m_FolderState = FolderState.Unchecked;
            m_ExpandedProperty = -1;
        }

        protected void OnDisable()
        {
            m_SerializedSettings = null;
        }

        protected void OnGUI()
        {
            settings.Update();
            InspectLocationSettings();
            InspectCurrentSave();
            settings.ApplyModifiedProperties();
        }

        #region LOCATION SETTINGS

        void InspectLocationSettings()
        {
            // Boxed foldout
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            var foldoutProp = settings.FindProperty("expandLocationSettings");
            foldoutProp.boolValue = EditorGUILayout.Foldout(foldoutProp.boolValue, "Save Location Settings", true);
            if (foldoutProp.boolValue)
            {
                ++EditorGUI.indentLevel;

                var pathRootProp = settings.FindProperty("savePathRoot");
                var subFolderProp = settings.FindProperty("saveSubFolder");

                // Layout space for help box
                var infoRect = EditorGUILayout.BeginVertical(GUILayout.Height(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing));
                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
                infoRect.width -= 15;
                infoRect.x += 15;

                // CheckAdd for path root change
                int pathRootIndex = pathRootProp.enumValueIndex;
                EditorGUILayout.PropertyField(pathRootProp);
                if (pathRootProp.enumValueIndex != pathRootIndex)
                    m_FolderState = FolderState.Unchecked;

                // CheckAdd for subfolder change
                var input = EditorGUILayout.DelayedTextField("Sub-Folder", subFolderProp.stringValue);
                if (input != subFolderProp.stringValue)
                {
                    subFolderProp.stringValue = SaveGameUtilities.FilterPathString(input);
                    m_FolderState = FolderState.Unchecked;
                }

                // CheckAdd the folder
                if (m_FolderState == FolderState.Unchecked)
                {
                    settings.ApplyModifiedProperties();
                    string folder = GetSaveFolder();

                    if (!Directory.Exists(folder))
                    {
                        m_FolderState = FolderState.Invalid;
                    }
                    else
                    {
                        // CheckAdd for save files
                        var available = Directory.GetFiles(folder, "*.saveData", SearchOption.AllDirectories);
                        if (available != null & available.Length > 0)
                            m_FolderState = FolderState.ValidWithSaves;
                        else
                            m_FolderState = FolderState.ValidNoSaves;
                    }
                }

                // Show status
                switch (m_FolderState)
                {
                    case FolderState.Invalid:
                        GUI.color = Color.red;
                        EditorGUI.HelpBox(infoRect, "Path does not exist", MessageType.Error);
                        GUI.color = Color.white;
                        break;
                    case FolderState.ValidNoSaves:
                        GUI.color = Color.yellow;
                        EditorGUI.HelpBox(infoRect, "Path exists, but no saves found", MessageType.Warning);
                        GUI.color = Color.white;
                        break;
                    case FolderState.ValidWithSaves:
                        EditorGUI.HelpBox(infoRect, "Path exists and contains save files", MessageType.Info);
                        break;
                }

                --EditorGUI.indentLevel;
            }
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region CURRENT SAVE

        void InspectCurrentSave()
        {
            if (m_Reader == null)
                return;

            // Load a save file
            if (GUILayout.Button("Open Save File"))
            {
                var filePath = EditorUtility.OpenFilePanel("Open Save File", GetSaveFolder(), "saveData");
                if (filePath.Length != 0)
                {
                    // Stop serializing old
                    if (m_Reader.isDeserializing)
                    {
                        while (m_Reader.currentContext.contextType != SerializationContext.Root)
                            m_Reader.PopContext(m_Reader.currentContext.contextType, -1);
                        m_Reader.EndDeserialization();
                    }
                    
                    // Get the file's display name
                    m_CurrentState = Path.GetFileNameWithoutExtension(filePath);

                    // Start deserialization
                    using (var fStream = File.OpenRead(filePath))
                    {
                        m_Reader.ReadFromStream(fStream);
                        m_Reader.BeginDeserialization();
                    }
                    m_ContextStack.Clear();
                }
            }

            if (!m_Reader.isDeserializing)
            {
                EditorGUILayout.HelpBox("Please open a save file to inspect its contents", MessageType.Info);
            }
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Show save file
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Current Save File:", EditorStyles.boldLabel, GUILayout.Width(120));
                EditorGUILayout.LabelField(m_CurrentState);
                EditorGUILayout.EndHorizontal();

                // Show current context breadcrumbs
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Current Context:", EditorStyles.boldLabel, GUILayout.Width(120));
                EditorGUILayout.LabelField(currentContextType.ToString());
                EditorGUILayout.EndHorizontal();

                // Show navigation buttons
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Inspect Root Context"))
                {
                    // Pop back to root
                    while (m_Reader.currentContext.contextType != SerializationContext.Root)
                        m_Reader.PopContext(m_Reader.currentContext.contextType, -1);
                    // Modify context stack
                    m_ContextStack.Clear();
                    m_ExpandedProperty = -1;
                }
                if (GUILayout.Button(string.Format("Inspect Parent ({0})", parentContextType)))
                {
                    // Pop current context
                    if (m_Reader.currentContext.contextType != SerializationContext.Root)
                        m_Reader.PopContext(m_Reader.currentContext.contextType, -1);
                    // Modify context stack
                    if (m_ContextStack.Count > 0)
                        m_ContextStack.RemoveAt(m_ContextStack.Count - 1);
                    m_ExpandedProperty = -1;
                }
                EditorGUILayout.EndHorizontal();

                // Get the current context
                var context = m_Reader.currentContext;

                // Show search strings
                EditorGUILayout.LabelField("Search for keys", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();

                var searchString = EditorGUILayout.DelayedTextField("");
                if (!string.IsNullOrEmpty(searchString))
                    AddString(searchString);

                if (GUILayout.Button("Export search strings", GUILayout.Width(150)))
                {
                    var filePath = EditorUtility.SaveFilePanelInProject("Export Search Strings", "NeoSaveGameSearchStrings", "json", "Choose where to save the exported search strings");
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        var export = new DictionaryExport(m_KeyMap);
                        using (var stream = File.CreateText(filePath))
                        {
                            string exportString = JsonUtility.ToJson(export, true);
                            //Debug.Log(exportString);
                            stream.Write(exportString);
                        }
                    }
                }

                if (GUILayout.Button("Import search strings", GUILayout.Width(150)))
                {
                    var filePath = EditorUtility.OpenFilePanel("Export Search Strings", Application.dataPath, "json");
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        string json = File.ReadAllText(filePath);
                        var import = JsonUtility.FromJson<DictionaryExport>(json);
                        import.AddToDictionary(m_KeyMap);
                    }
                }

                EditorGUILayout.LabelField("Current search string count: " + m_KeyMap.Count, GUILayout.Width(200));

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();

                m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
                // Inspect context properties
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);

                if (context.properties.Count > 0)
                {
                    // Show column headers
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Property Name:", GUILayout.Width(k_NameWidth + 4));
                    EditorGUILayout.LabelField("Hash:", GUILayout.Width(k_IdWidth));
                    EditorGUILayout.LabelField("Property Type:", GUILayout.Width(k_TypeWidth));
                    EditorGUILayout.LabelField("IValue:");
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    // Show individual properties
                    foreach (var kvp in context.properties)
                    {
                        InspectProperty(kvp.Value, kvp.Key);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No properties found");
                }

                EditorGUILayout.EndVertical();

                // Inspect sub-contexts
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Sub-Contexts", EditorStyles.boldLabel);

                if (context.subContexts.Count > 0)
                {
                    // Show column headers
                    EditorGUILayout.BeginHorizontal();
                    //EditorGUILayout.LabelField("Context Name:", GUILayout.Width(k_NameWidth + 4));
                    EditorGUILayout.LabelField("Hash/ID:", GUILayout.Width(k_IdWidth + 4));
                    EditorGUILayout.LabelField("Context Type:", GUILayout.Width(k_TypeWidth));
                    EditorGUILayout.LabelField("Contents:", GUILayout.Width(k_ContentsWidth));
                    EditorGUILayout.LabelField("Inspect:");
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    // Show individual contexts
                    foreach (var kvp in context.subContexts)
                    {
                        if (!InspectContext(kvp.Value))
                            break;
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No sub-contexts found");
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndScrollView();
            }

        }

        #endregion

        #region ELEMENTS

        const float k_NameWidth = 200;
        const float k_IdWidth = 120;
        const float k_TypeWidth = 160;
        const float k_ContentsWidth = 200;

        void InspectProperty(BinaryDeserializer.Property prop, int id)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.textArea);

            // Display name (if appropriate)
            string key;
            if (m_KeyMap.TryGetValue(id, out key))
            {
                GUI.color = Color.green;
                EditorGUILayout.LabelField(key, GUILayout.Width(k_NameWidth));
                GUI.color = Color.white;
            }
            else
                EditorGUILayout.LabelField("<Unknown>", GUILayout.Width(k_NameWidth));

            // Display ID - Selectableabel
            EditorGUILayout.SelectableLabel(id.ToString(), GUILayout.Width(k_IdWidth), GUILayout.Height(EditorGUIUtility.singleLineHeight));

            // Display Type
            var propertyType = prop.propertyType;
            string typeLabel = prop.propertyType.ToString();
            if (prop.isArray)
                typeLabel += " Array";
            EditorGUILayout.LabelField(typeLabel, GUILayout.Width(k_TypeWidth));

            if (prop.isArray)
                InspectArrayPropertyValue(propertyType, id, prop.isNullOrEmpty);
            else
                InspectPropertyValue(propertyType, id);

            EditorGUILayout.EndHorizontal();
        }

        bool InspectContext(BinaryDeserializer.Context context)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.textArea);
            
            EditorGUILayout.SelectableLabel(context.id.ToString(), GUILayout.Width(k_IdWidth), GUILayout.Height(EditorGUIUtility.singleLineHeight));

            EditorGUILayout.LabelField(context.contextType.ToString(), GUILayout.Width(k_TypeWidth));

            EditorGUILayout.LabelField(string.Format("{0} properties, {1} sub-contexts", context.properties.Count, context.subContexts.Count), GUILayout.Width(k_ContentsWidth));

            if (GUILayout.Button("Inspect"))
            {
                try
                {
                    m_Reader.PushContext(context.contextType, context.id);
                    m_ContextStack.Add(context.contextType);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error getting sub-context: " + e.Message);
                }
                m_ExpandedProperty = -1;
                return false;
            }

            EditorGUILayout.EndHorizontal();
            return true;
        }

        #endregion

        #region PROPERTY VALUES

        delegate string PropertyFormatter<T>(T prop);

        void InspectPropertyValue(PropertyType p, int id)
        {
            var h = GUILayout.Height(EditorGUIUtility.singleLineHeight);
            switch (p)
            {
                case PropertyType.Bool:
                    {
                        bool print;
                        m_Reader.TryReadValue(id, out print, false);
                        EditorGUILayout.SelectableLabel(print.ToString(), h);
                    }
                    break;
                case PropertyType.Byte:
                    {
                        byte print;
                        m_Reader.TryReadValue(id, out print, 0);
                        EditorGUILayout.SelectableLabel(print.ToString(), h);
                    }
                    break;
                case PropertyType.SignedByte:
                    {
                        sbyte print;
                        m_Reader.TryReadValue(id, out print, 0);
                        EditorGUILayout.SelectableLabel(print.ToString(), h);
                    }
                    break;
                case PropertyType.Short:
                    {
                        short print;
                        m_Reader.TryReadValue(id, out print, 0);
                        EditorGUILayout.SelectableLabel(print.ToString(), h);
                    }
                    break;
                case PropertyType.UnsignedShort:
                    {
                        ushort print;
                        m_Reader.TryReadValue(id, out print, 0);
                        EditorGUILayout.SelectableLabel(print.ToString(), h);
                    }
                    break;
                case PropertyType.Int:
                    {
                        int print;
                        m_Reader.TryReadValue(id, out print, 0);
                        EditorGUILayout.SelectableLabel(print.ToString(), h);
                    }
                    break;
                case PropertyType.UnsignedInt:
                    {
                        uint print;
                        m_Reader.TryReadValue(id, out print, 0);
                        EditorGUILayout.SelectableLabel(print.ToString(), h);
                    }
                    break;
                case PropertyType.Long:
                    {
                        long print;
                        m_Reader.TryReadValue(id, out print, 0);
                        EditorGUILayout.SelectableLabel(print.ToString(), h);
                    }
                    break;
                case PropertyType.UnsignedLong:
                    {
                        ulong print;
                        m_Reader.TryReadValue(id, out print, 0);
                        EditorGUILayout.SelectableLabel(print.ToString(), h);
                    }
                    break;
                case PropertyType.Float:
                    {
                        float print;
                        m_Reader.TryReadValue(id, out print, 0);
                        EditorGUILayout.SelectableLabel(print.ToString(), h);
                    }
                    break;
                case PropertyType.Double:
                    {
                        double print;
                        m_Reader.TryReadValue(id, out print, 0);
                        EditorGUILayout.SelectableLabel(print.ToString(), h);
                    }
                    break;
                case PropertyType.Vector2:
                    {
                        Vector2 print;
                        m_Reader.TryReadValue(id, out print, Vector2.zero);
                        EditorGUILayout.SelectableLabel(print.ToString("F3"), h);
                    }
                    break;
                case PropertyType.Vector3:
                    {
                        Vector3 print;
                        m_Reader.TryReadValue(id, out print, Vector3.zero);
                        EditorGUILayout.SelectableLabel(print.ToString("F3"), h);
                    }
                    break;
                case PropertyType.Vector4:
                    {
                        Vector4 print;
                        m_Reader.TryReadValue(id, out print, Vector4.zero);
                        EditorGUILayout.SelectableLabel(print.ToString("F3"), h);
                    }
                    break;
                case PropertyType.Vector2Int:
                    {
                        Vector2Int print;
                        m_Reader.TryReadValue(id, out print, Vector2Int.zero);
                        EditorGUILayout.SelectableLabel(print.ToString(), h);
                    }
                    break;
                case PropertyType.Vector3Int:
                    {
                        Vector3Int print;
                        m_Reader.TryReadValue(id, out print, Vector3Int.zero);
                        EditorGUILayout.SelectableLabel(print.ToString(), h);
                    }
                    break;
                case PropertyType.Quaternion:
                    {
                        Quaternion print;
                        m_Reader.TryReadValue(id, out print, Quaternion.identity);
                        EditorGUILayout.SelectableLabel(print.ToString("F3"), h);
                    }
                    break;
                case PropertyType.Color:
                    {
                        Color print;
                        m_Reader.TryReadValue(id, out print, Color.black);
                        EditorGUILayout.SelectableLabel(print.ToString("F3"), h);
                    }
                    break;
                case PropertyType.String:
                    {
                        string print;
                        m_Reader.TryReadValue(id, out print, string.Empty);
                        EditorGUILayout.SelectableLabel(print, h);
                    }
                    break;
                case PropertyType.DateTime:
                    {
                        DateTime print;
                        m_Reader.TryReadValue(id, out print, new DateTime());
                        EditorGUILayout.SelectableLabel(string.Format("{0} {1}", print.ToShortDateString(), print.ToShortTimeString()), h);
                    }
                    break;
                case PropertyType.Guid:
                    {
                        Guid print;
                        m_Reader.TryReadValue(id, out print);
                        EditorGUILayout.SelectableLabel(print.ToString(), h);
                    }
                    break;
                default:
                    {
                        EditorGUILayout.LabelField("Complex IValue");
                    }
                    break;
            }
        }

        void PrintArrayValues<T>(T[] array)
        {
            // CheckAdd if array is empty
            if (array.Length == 0)
                EditorGUILayout.LabelField("<Empty>");
            else
            {
                // Enforce rows rows (selectable labels are tall for some reason)
                var h = GUILayout.Height(EditorGUIUtility.singleLineHeight);

                // Loop through to end or 50, whichever comes first
                int i = 0;
                for (; i < array.Length && i < 50; ++i)
                    EditorGUILayout.SelectableLabel(array[i].ToString(), h);
                if (i == 50)
                    EditorGUILayout.LabelField("...");
            }
        }

        void PrintArrayValues<T>(T[] array, PropertyFormatter<T> formatter)
        {
            // CheckAdd if array is empty
            if (array.Length == 0)
                EditorGUILayout.LabelField("<Empty>");
            else
            {
                // Enforce rows rows (selectable labels are tall for some reason)
                var h = GUILayout.Height(EditorGUIUtility.singleLineHeight);

                // Loop through to end or 50, whichever comes first
                int i = 0;
                for (; i < array.Length && i < 50; ++i)
                    EditorGUILayout.SelectableLabel(formatter(array[i]), h);
                if (i == 50)
                    EditorGUILayout.LabelField("...");
            }
        }

        void InspectArrayPropertyValue(PropertyType p, int id, bool isNullOrEmpty)
        {
            if (isNullOrEmpty)
            {
                EditorGUILayout.LabelField("<Null>");
                return;
            }

            EditorGUILayout.BeginVertical();

            bool expanded = (id == m_ExpandedProperty);
            if (EditorGUILayout.Foldout(expanded, "Array Values", true))
            {
                ++EditorGUI.indentLevel;

                m_ExpandedProperty = id;
                
                switch (p)
                {
                    case PropertyType.Bool:
                        {
                            bool[] print = new bool[0];
                            m_Reader.TryReadValues(id, out print, print);
                            PrintArrayValues(print);
                        }
                        break;
                    case PropertyType.Byte:
                        {
                            byte[] print = new byte[0];
                            m_Reader.TryReadValues(id, out print, print);
                            PrintArrayValues(print);
                        }
                        break;
                    case PropertyType.SignedByte:
                        {
                            sbyte[] print = new sbyte[0];
                            m_Reader.TryReadValues(id, out print, print);
                            PrintArrayValues(print);
                        }
                        break;
                    case PropertyType.Short:
                        {
                            short[] print = new short[0];
                            m_Reader.TryReadValues(id, out print, print);
                            PrintArrayValues(print);
                        }
                        break;
                    case PropertyType.UnsignedShort:
                        {
                            ushort[] print = new ushort[0];
                            m_Reader.TryReadValues(id, out print, print);
                            PrintArrayValues(print);
                        }
                        break;
                    case PropertyType.Int:
                        {
                            int[] print = new int[0];
                            m_Reader.TryReadValues(id, out print, print);
                            PrintArrayValues(print);
                        }
                        break;
                    case PropertyType.UnsignedInt:
                        {
                            uint[] print = new uint[0];
                            m_Reader.TryReadValues(id, out print, print);
                            PrintArrayValues(print);
                        }
                        break;
                    case PropertyType.Long:
                        {
                            long[] print = new long[0];
                            m_Reader.TryReadValues(id, out print, print);
                            PrintArrayValues(print);
                        }
                        break;
                    case PropertyType.UnsignedLong:
                        {
                            ulong[] print = new ulong[0];
                            m_Reader.TryReadValues(id, out print, print);
                            PrintArrayValues(print);
                        }
                        break;
                    case PropertyType.Float:
                        {
                            float[] print = new float[0];
                            m_Reader.TryReadValues(id, out print, print);
                            PrintArrayValues(print, (float prop) => { return prop.ToString("F5"); });
                        }
                        break;
                    case PropertyType.Double:
                        {
                            double[] print = new double[0];
                            m_Reader.TryReadValues(id, out print, print);
                            PrintArrayValues(print, (double prop) => { return prop.ToString("F5"); });
                        }
                        break;
                    case PropertyType.Vector2:
                        {
                            Vector2[] print = new Vector2[0];
                            m_Reader.TryReadValues(id, out print, print);
                            PrintArrayValues(print, (Vector2 prop) => { return prop.ToString("F3"); });
                        }
                        break;
                    case PropertyType.Vector3:
                        {
                            Vector3[] print = new Vector3[0];
                            m_Reader.TryReadValues(id, out print, print);
                            PrintArrayValues(print, (Vector3 prop) => { return prop.ToString("F3"); });
                        }
                        break;
                    case PropertyType.Vector4:
                        {
                            Vector4[] print = new Vector4[0];
                            m_Reader.TryReadValues(id, out print, print);
                            PrintArrayValues(print, (Vector4 prop) => { return prop.ToString("F3"); });
                        }
                        break;
                    case PropertyType.Vector2Int:
                        {
                            Vector2Int[] print = new Vector2Int[0];
                            m_Reader.TryReadValues(id, out print, print);
                            PrintArrayValues(print);
                        }
                        break;
                    case PropertyType.Vector3Int:
                        {
                            Vector3Int[] print = new Vector3Int[0];
                            m_Reader.TryReadValues(id, out print, print);
                            PrintArrayValues(print);
                        }
                        break;
                    case PropertyType.Quaternion:
                        {
                            Quaternion[] print = new Quaternion[0];
                            m_Reader.TryReadValues(id, out print, print);
                            PrintArrayValues(print, (Quaternion prop) => { return prop.ToString("F3"); });
                        }
                        break;
                    case PropertyType.Color:
                        {
                            Color[] print = new Color[0];
                            m_Reader.TryReadValues(id, out print, print);
                            PrintArrayValues(print, (Color prop) => { return prop.ToString("F3"); });
                        }
                        break;
                    case PropertyType.String:
                        {
                            string[] print = new string[0];
                            m_Reader.TryReadValues(id, out print, print);
                            PrintArrayValues(print);
                        }
                        break;
                    case PropertyType.Guid:
                        {
                            Guid[] print;
                            m_Reader.TryReadValues(id, out print);
                            PrintArrayValues(print);
                        }
                        break;
                    default:
                        {
                            EditorGUILayout.LabelField("Complex IValue");
                        }
                        break;
                }
                --EditorGUI.indentLevel;
            }
            else
            {
                if (expanded)
                    m_ExpandedProperty = -1;
            }

            EditorGUILayout.EndVertical();
        }

        #endregion

        #region KEY MAP

        void AddString(string key)
        {
            int hash = NeoSerializationUtilities.StringToHash(key);
            //Debug.Log(string.Format("Adding string: {0}, hash: {1}", key, hash));
            if (!m_KeyMap.ContainsKey(hash))
                m_KeyMap.Add(hash, key);
        }

        void AddDefaultKeyMaps()
        {
            AddString("name");
            AddString("active");
            AddString("position");
            AddString("rotation");
            AddString("scale");
            AddString("destroyedObjects");
            AddString("runtimeObjects");
            AddString("saveType");
            AddString("title");
            AddString("saveTime");
            AddString("hasThumbnail");
            AddString("thumbnailSize");
            AddString("thumbnailFormat");
            AddString("thumbnailData");
            AddString("mainScene");
            AddString("subScenes");
        }

        #endregion

        string GetSaveFolder()
        {
            switch (s_Settings.savePathRoot)
            {
                case SavePathRoot.PersistantDataPath:
                    if (string.IsNullOrEmpty(s_Settings.saveSubFolder))
                        return Application.persistentDataPath + '/';
                    else
                        return string.Format("{0}/{1}/", Application.persistentDataPath, s_Settings.saveSubFolder);
                case SavePathRoot.DataPath:
                    if (string.IsNullOrEmpty(s_Settings.saveSubFolder))
                        return Application.dataPath + '/';
                    else
                        return string.Format("{0}/{1}/", Application.dataPath, s_Settings.saveSubFolder);
                default:
                    return s_Settings.saveSubFolder;
            }
        }
    }
}

#endif