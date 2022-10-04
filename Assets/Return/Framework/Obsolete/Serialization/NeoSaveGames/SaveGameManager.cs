using NeoSaveGames.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using NeoSaveGames.SceneManagement;
using Return;

namespace NeoSaveGames
{
    [CreateAssetMenu(fileName = "FpsManager_SaveGames")]
    [HelpURL("https://docs.neofps.com/manual/savegamesref-so-savegamemanager.html")]
    public class SaveGameManager : SingletonSO<SaveGameManager>
    {
        private static readonly NeoSerializationKey k_MainSceneKey = new NeoSerializationKey("mainScene");
        private static readonly NeoSerializationKey k_SubScenesKey = new NeoSerializationKey("subScenes");

        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void LoadSaveGameManager()
        {
            GetInstance("FpsManager_SaveGames");
        }
        public override void Initialize()
        {
            GetBehaviourProxy<RuntimeBehaviour>();

            // Check folder exists
            CheckSaveFolder();

            // Initialise
            InitialiseThreading();
            InitialiseSerialization();
            RefreshAvailableSaves();

            // RegisterHandler dynamic objects
            NeoSerializedObjectFactory.RegisterPrefabs(m_Prefabs);
            for (int i = 0; i < m_Assets.Length; ++i)
            {
                var cast = m_Assets[i] as INeoSerializableAsset;
                if (cast != null)
                    NeoSerializedObjectFactory.RegisterAsset(cast);
            }
        }

        public override bool IsValid()
        {
            return true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unregister dynamic objects
            NeoSerializedObjectFactory.UnregisterPrefabs(m_Prefabs);
            for (int i = 0; i < m_Assets.Length; ++i)
            {
                var cast = m_Assets[i] as INeoSerializableAsset;
                if (cast != null)
                    NeoSerializedObjectFactory.UnregisterAsset(cast);
            }

            DestroyThreading();
        }

        #region THREADING

        private Thread m_Thread = null;
        private EventWaitHandle m_ThreadWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private volatile bool m_Destroyed = false;
        private bool m_SideThreadBusy = false;
        private bool m_MainThreadBusy = false;
        private object m_LockObject = new object();
        private Queue<AsyncSaveLoadJob> m_JobQueue = new Queue<AsyncSaveLoadJob>();
        private IEnumerator m_MainThreadTask = null;

        public static bool inProgress
        {
            get
            {
                if (Instance == null)
                    return false;

                if (Instance.m_MainThreadBusy)
                    return true;

                // Locks might not be required, depending on if optimisations
                // mess with order of instructions in thread loop
                lock (Instance.m_LockObject)
                    return Instance.m_SideThreadBusy;
            }
        }

        class RuntimeBehaviour : MonoBehaviour
        {
            protected void Start()
            {
                StartCoroutine(Instance.RefreshAvailableSaves());
            }

            protected void Update()
            {
                if (Instance.m_MainThreadTask != null)
                    StartCoroutine(TaskCoroutine());
            }

            IEnumerator TaskCoroutine()
            {
                yield return StartCoroutine(Instance.m_MainThreadTask);
                Instance.m_MainThreadTask = null;
                Instance.m_ThreadWaitHandle.Set();
            }
        }

        void InitialiseThreading()
        {
            m_Thread = new Thread(ThreadLoop)
            {
                Name = "NeoFPS Save System Thread",
                IsBackground = true
            };
            m_Thread.Start();

            // Reset variables that might have been muddied by the editor
            m_SideThreadBusy = false;
            m_MainThreadBusy = false;
            m_JobQueue.Clear();
            m_MainThreadTask = null;
        }

        void DestroyThreading()
        {
            m_Destroyed = true;
            m_ThreadWaitHandle.Set();

            bool stopped = false;
            try
            {
                if (!m_Thread.Join(TimeSpan.FromSeconds(15)))
                {
                    Debug.Log("Unable to stop save game thread gracefully");
                }
                else
                    stopped = true;
            }
            finally
            {
                // abort in case it has not been stopped yet
                if (!stopped)
                    m_Thread.Abort();
                // dispose the wait handle because it would otherwise leak memory
                m_ThreadWaitHandle.Dispose();
            }
        }

        void ThreadLoop()
        {
            while (!m_Destroyed)
            {
                lock (m_LockObject)
                    m_SideThreadBusy = false;

                m_ThreadWaitHandle.WaitOne();

                lock (m_LockObject)
                    m_SideThreadBusy = true;

                // Clear the job pool
                while (true)
                {
                    var job = GetJob();

                    if (job != null)
                        job.Start();
                    else
                        break;
                }
            }

            var thread = m_Thread;
            m_Thread = null;
            thread.Abort();
        }

        void AddJob(AsyncSaveLoadJob job)
        {
            lock (m_LockObject)
                m_JobQueue.Enqueue(job);
            m_ThreadWaitHandle.Set();
        }

        AsyncSaveLoadJob GetJob()
        {
            lock (m_LockObject)
            {
                if (m_JobQueue.Count > 0)
                    return m_JobQueue.Dequeue();
            }
            return null;
        }

        private abstract class AsyncSaveLoadJob
        {
            public SaveGameManager manager
            {
                get;
                private set;
            }

            public bool completed
            {
                get;
                protected set;
            }

            public AsyncSaveLoadJob(SaveGameManager m)
            {
                manager = m;
                completed = false;
            }

            public void Start()
            {
                while (!completed)
                    Process();
            }

            protected void WaitOnMainThreadTask(IEnumerator task)
            {
                manager.m_MainThreadTask = task;
                manager.m_ThreadWaitHandle.WaitOne();
            }

            protected abstract void Process();
            public abstract void Abort();
        }

        #endregion


        #region SERIALIZATION

        public INeoSerializer serializer
        {
            get;
            private set;
        }

        public INeoDeserializer deserializer
        {
            get;
            private set;
        }

        void InitialiseSerialization()
        {
#if UNITY_WEBGL
            serializer = new SafeSerializer();
            deserializer = new SafeDeserializer();
#else
            serializer = new BinarySerializer();
            deserializer = new BinaryDeserializer();
#endif
        }

        #endregion


        #region VALIDATION

        protected void OnValidate()
        {
            ValidatePrefabs();
            ValidateAssets();
            ValidatePath();
        }

        void ValidatePrefabs()
        {
            int valid = 0;
            for (int i = 0; i < m_Prefabs.Length; ++i)
            {
                if (m_Prefabs[i] != null)
                    ++valid;
            }
            if (valid != m_Prefabs.Length)
            {
                var rebuilt = new NeoSerializedGameObject[valid];
                int itr = 0;
                for (int i = 0; i < m_Prefabs.Length; ++i)
                {
                    if (m_Prefabs[i] != null)
                    {
                        rebuilt[itr] = m_Prefabs[i];
                        ++itr;
                    }
                }
                m_Prefabs = rebuilt;
            }
        }

        void ValidateAssets()
        {
            int valid = 0;
            for (int i = 0; i < m_Assets.Length; ++i)
            {
                if (m_Assets[i] != null)
                    ++valid;
            }
            if (valid != m_Assets.Length)
            {
                var rebuilt = new ScriptableObject[valid];
                int itr = 0;
                for (int i = 0; i < m_Assets.Length; ++i)
                {
                    if (m_Assets[i] != null)
                    {
                        rebuilt[itr] = m_Assets[i];
                        ++itr;
                    }
                }
                m_Assets = rebuilt;
            }
        }

        #endregion


        #region REGISTERED ITEMS

        [SerializeField, Tooltip("")]
        private NeoSerializedGameObject[] m_Prefabs = new NeoSerializedGameObject[0];

        [SerializeField, Tooltip("")]
        private ScriptableObject[] m_Assets = new ScriptableObject[0];

#if UNITY_EDITOR

        public bool CheckIsPrefabRegistered(NeoSerializedGameObject prefab)
        {
            if (m_Prefabs != null && prefab != null)
            {
                for (int i = 0; i < m_Prefabs.Length; ++i)
                {
                    if (m_Prefabs[i] != null && m_Prefabs[i].prefabStrongID == prefab.prefabStrongID)
                        return true;
                }
            }
            return false;
            //return (Array.IndexOf(m_Prefabs, prefab) != -1);
        }

        public void RegisterPrefab(NeoSerializedGameObject prefab)
        {
            if (Array.IndexOf(m_Prefabs, prefab) == -1)
            {
                var so = new UnityEditor.SerializedObject(this);
                var prop = so.FindProperty("m_Prefabs");
                ++prop.arraySize;
                prop = prop.GetArrayElementAtIndex(prop.arraySize - 1);
                prop.objectReferenceValue = prefab;
                so.ApplyModifiedProperties();
            }
        }

#endif

        #endregion


        #region QUICK SAVE

        [Header("Quick-Save")]
        [SerializeField, Tooltip("Sets whether the quick-save system is enabled in this project")]
        private bool m_CanQuickSave = true;
        [SerializeField, Tooltip("If true, quick loading will load the latest quick/auto/manual save. If not then it will only load the latest quick-save")]
        private bool m_QuickLoadAll = true;
        [SerializeField, Range(1, 10), Tooltip("The number of quicksaves to maintain. If the number exceeds this value, the oldest saves will be deleted.")]
        private int m_NumQuicksaves = 3;

        public static bool quickSaveEnabled
        {
            get { return Instance != null && Instance.m_CanQuickSave; }
        }

        public static bool canQuickSave
        {
            get { return Instance != null && Instance.m_CanQuickSave && !inProgress && Instance.m_MainScene != null; }
        }

        public static bool canQuickLoad
        {
            get
            {
                if (Instance == null || inProgress)
                    return false;

                if (Instance.m_QuickLoadAll)
                    return hasAvailableSaves;
                else
                    return (availableQuicksaves.Length > 0);
            }
        }

        public static bool QuickSave()
        {
            if (canQuickSave)
                return Instance.SaveGameInternal(SaveGameType.Quicksave, mainScene.displayName, null);
            else
            {
                if (onSaveFailed != null)
                    onSaveFailed(SaveGameType.Quicksave);
                return false;
            }
        }

        public static bool QuickLoad()
        {
            if (canQuickLoad)
            {
                if (Instance.m_QuickLoadAll)
                    return LoadGame(GetLatestSave(SaveGameTypeFilter.All));
                else
                    return LoadGame(GetLatestSave(SaveGameTypeFilter.Quicksave));
            }
            else
                return false;
        }

        #endregion


        #region AUTO SAVE

        [Header("Auto-Save")]
        [SerializeField, Range(1, 10), Tooltip("The number of autosaves to maintain. If the number exceeds this value, the oldest saves will be deleted.")]
        private int m_NumAutosaves = 3;

        public static bool canAutoSave
        {
            get { return Instance != null && !inProgress && Instance.m_MainScene != null; }
        }

        public static bool canAutoLoad
        {
            get { return Instance != null && availableAutosaves.Length > 0 && !inProgress; }
        }

        public static bool AutoSave()
        {
            if (canAutoSave)
                return Instance.SaveGameInternal(SaveGameType.Autosave, mainScene.displayName, null);
            else
            {
                if (onSaveFailed != null)
                    onSaveFailed(SaveGameType.Autosave);
                return false;
            }
        }

        public static bool AutoLoad()
        {
            if (canAutoLoad)
                return LoadGame(GetLatestSave(SaveGameTypeFilter.Autosave));
            else
                return false;
        }

        #endregion


        #region MANUAL SAVE

        [Header("Manual Save")]
        [SerializeField, Tooltip("Sets whether the player can manually save the game in this project")]
        private bool m_CanManualSave = true;

        public static bool manualSaveEnabled
        {
            get { return Instance != null && Instance.m_CanManualSave; }
        }

        public static bool canManualSave
        {
            get { return Instance != null && Instance.m_CanManualSave && !inProgress && Instance.m_MainScene != null; }
        }

        public static bool SaveGame(string title, FileInfo replaces = null)
        {
            if (!canManualSave)
            {
                if (onSaveFailed != null)
                    onSaveFailed(SaveGameType.Manual);
                return false;
            }

            return Instance.SaveGameInternal(SaveGameType.Manual, title, () =>
            {
                if (replaces != null)
                    replaces.Delete();
            });
        }

        #endregion


        #region CONTINUE

        [Header("Continue")]
        [SerializeField, Tooltip("The BindingTypes of saves to use when continuing gameplay from the main menu")]
        private ContinueType m_ContinueFrom = ContinueType.AutoSaveOnly;

        public enum ContinueType
        {
            None,
            All,
            AutoSaveOnly
        }

        public static bool canContinue
        {
            get
            {
                if (Instance == null || inProgress)
                    return false;

                switch (Instance.m_ContinueFrom)
                {
                    case ContinueType.None:
                        return false;
                    case ContinueType.AutoSaveOnly:
                        return availableAutosaves.Length > 0;
                    case ContinueType.All:
                        return hasAvailableSaves;
                }

                return false;
            }
        }

        public static bool Continue()
        {
            if (canContinue)
            {
                switch (Instance.m_ContinueFrom)
                {
                    case ContinueType.AutoSaveOnly:
                        return LoadGame(GetLatestSave(SaveGameTypeFilter.Autosave));
                    case ContinueType.All:
                        return LoadGame(GetLatestSave(SaveGameTypeFilter.All));
                    default:
                        return false;
                }
            }
            else
                return false;
        }

        #endregion


        #region SAVE TO BUFFER

        private static MemoryStream s_TempDataStream = null;
        private static readonly NeoSerializationKey k_KeysKey = new NeoSerializationKey("keys");

        public static bool SaveGameObjectsToBuffer(NeoSerializedGameObject[] objects, SaveMode saveMode)
        {
            // Basic checks (since anyone can call this)
            if (Instance == null || inProgress || objects == null)
                return false;

            // Begin the serialization process
            var writer = Instance.serializer;
            writer.BeginSerialization();

            // Write an array of keys in the order provided
            Vector2Int[] keys = new Vector2Int[objects.Length];
            for (int i = 0; i < objects.Length; ++i)
            {
                if (objects[i] == null)
                {
                    keys[i] = Vector2Int.zero;
                }
                else
                {
                    if (!objects[i].wasRuntimeInstantiated || objects[i].prefabStrongID == 0)
                    {
                        Debug.LogError("Game objects saved to buffer must be runtime instantiated prefab Instances in order to be rebuilt correctly in the new scene. Invalid: " + objects[i].name);
                        writer.EndSerialization();
                        return false;
                    }
                    keys[i].x = objects[i].serializationKey;
                    keys[i].y = objects[i].prefabStrongID;
                }
            }
            writer.WriteValues(k_KeysKey, keys);

            // Serialize each of the game objects
            for (int i = 0; i < objects.Length; ++i)
            {
                if (objects[i] != null)
                {
                    writer.PushContext(SerializationContext.GameObject, objects[i].serializationKey);
                    objects[i].WriteGameObject(writer, saveMode);
                    writer.PopContext(SerializationContext.GameObject);
                }
            }

            // End the serialization process and write to stream
            writer.EndSerialization();
            s_TempDataStream = new MemoryStream(writer.byteLength);
            writer.WriteToStream(s_TempDataStream);
            s_TempDataStream.Position = 0;

            return true;
        }

        public static NeoSerializedGameObject[] LoadGameObjectsFromBuffer(NeoSerializedGameObjectContainerBase container)
        {
            if (Instance == null || inProgress || s_TempDataStream == null)
                return null;

            // Read from stream
            var reader = Instance.deserializer;
            reader.ReadFromStream(s_TempDataStream);
            //s_TempDataStream.Close();
            s_TempDataStream = null;

            // Begin the deserialization process
            reader.BeginDeserialization();

            // Read the keys array
            Vector2Int[] keys = null;
            if (!reader.TryReadValues(k_KeysKey, out keys, null))
                return null;

            // Instantiate each of the objects
            var results = new NeoSerializedGameObject[keys.Length];
            for (int i = 0; i < keys.Length; ++i)
            {
                if (keys[i].x == 0)
                    results[i] = null;
                else
                    results[i] = NeoSerializedObjectFactory.Instantiate(keys[i].y, keys[i].x, container);
            }

            // Deserialize each of the game objects
            for (int i = 0; i < results.Length; ++i)
            {
                if (results[i] != null)
                {
                    if (reader.PushContext(SerializationContext.GameObject, results[i].serializationKey))
                    {
                        results[i].ReadGameObjectHierarchy(reader);
                        reader.PopContext(SerializationContext.GameObject, results[i].serializationKey);
                    }
                }
            }
            for (int i = 0; i < results.Length; ++i)
            {
                if (results[i] != null)
                {
                    if (reader.PushContext(SerializationContext.GameObject, results[i].serializationKey))
                    {
                        results[i].ReadGameObjectProperties(reader);
                        reader.PopContext(SerializationContext.GameObject, results[i].serializationKey);
                    }
                }
            }

            // End the deserialization process
            reader.EndDeserialization();

            return results;
        }

        public static bool LoadGameObjectsFromBuffer(NeoSerializedGameObject[] objects)
        {
            if (Instance == null || inProgress || objects == null || s_TempDataStream == null)
                return false;

            // Read from stream
            var reader = Instance.deserializer;
            reader.ReadFromStream(s_TempDataStream);
            //s_TempDataStream.Close();
            s_TempDataStream = null;

            // Begin the deserialization process
            reader.BeginDeserialization();

            // Deserialize each of the game objects
            for (int i = 0; i < objects.Length; ++i)
            {
                if (objects[i] != null)
                {
                    if (reader.PushContext(SerializationContext.GameObject, i))
                    {
                        objects[i].ReadGameObjectHierarchy(reader);
                        reader.PopContext(SerializationContext.GameObject, i);
                    }
                }
            }
            for (int i = 0; i < objects.Length; ++i)
            {
                if (objects[i] != null)
                {
                    if (reader.PushContext(SerializationContext.GameObject, i))
                    {
                        objects[i].ReadGameObjectProperties(reader);
                        reader.PopContext(SerializationContext.GameObject, i);
                    }
                }
            }

            // End the deserialization process
            reader.EndDeserialization();

            return false;
        }

        #endregion


        #region THUMBNAIL

        [Header("Thumbnails")]
        [SerializeField, Tooltip("Where to get the tumbnail texture for saved scenes")]
        private Thumbnail m_QuicksaveThumbnail = Thumbnail.None;
        [SerializeField, Tooltip("The thumbnail to use for saved scenes")]
        private Texture2D m_QsThumbnailTexture = null;
        [SerializeField, Tooltip("Where to get the tumbnail texture for saved scenes")]
        private Thumbnail m_AutosaveThumbnail = Thumbnail.None;
        [SerializeField, Tooltip("The thumbnail to use for saved scenes")]
        private Texture2D m_AsThumbnailTexture = null;
        [SerializeField, Tooltip("Where to get the tumbnail texture for saved scenes")]
        private Thumbnail m_ManualSaveThumbnail = Thumbnail.None;
        [SerializeField, Tooltip("The thumbnail to use for saved scenes")]
        private Texture2D m_MsThumbnailTexture = null;
        [SerializeField, Tooltip("The lines of any save game screenshots")]
        private Vector2Int m_ScreenshotSize = new Vector2Int(256, 256);
        [SerializeField, Tooltip("Should the save game screenshot be compressed")]
        private bool m_ScreenshotCompression = true;
        [SerializeField, Tooltip("Is the game using linear rendering? Screenshots need to match")]
        private bool m_UsingLinearRendering = true;

        public enum Thumbnail
        {
            None,
            Texture,
            TextureFromScene,
            Screenshot
        }

        public Texture2D GetThumbnail(SaveGameType saveType)
        {
            Thumbnail thumbnail = Thumbnail.None;
            Texture2D thumbnailTexture = null;

            // Get settings from type
            switch (saveType)
            {
                case SaveGameType.Quicksave:
                    thumbnail = m_QuicksaveThumbnail;
                    thumbnailTexture = m_QsThumbnailTexture;
                    break;
                case SaveGameType.Autosave:
                    thumbnail = m_AutosaveThumbnail;
                    thumbnailTexture = m_AsThumbnailTexture;
                    break;
                case SaveGameType.Manual:
                    thumbnail = m_ManualSaveThumbnail;
                    thumbnailTexture = m_MsThumbnailTexture;
                    break;
            }

            // Get texture
            switch (thumbnail)
            {
                case Thumbnail.TextureFromScene:
                    if (mainScene != null && mainScene.thumbnailTexture != null)
                        thumbnailTexture = mainScene.thumbnailTexture;
                    break;
                case Thumbnail.Screenshot:
                    var screenshot = GetScreenshot(m_ScreenshotSize, m_ScreenshotCompression);
                    if (screenshot != null)
                        thumbnailTexture = screenshot;
                    break;
            }

            return thumbnailTexture;
        }

        Texture2D GetScreenshot(Vector2Int size, bool compressed)
        {
            // Based on: https://pastebin.com/qkkhWs2J

            var capture = ScreenCapture.CaptureScreenshotAsTexture();

            // - Could try replacing with a version which renders the main camera direct into a render texture
            // - Or use ReadPixels() instead of ScreenCapture

            // We need the source texture in VRAM because we render with it
            capture.filterMode = FilterMode.Bilinear;
            //capture.alphaIsTransparency = true;
            capture.Apply(true);

            // Set the RTT in order to render to it
            RenderTexture rtt = new RenderTexture(size.x, size.y, 32);
            rtt.autoGenerateMips = false;
            Graphics.SetRenderTarget(rtt);

            // RegisterHandler 2D matrix in range 0..1, so nobody needs to care about sized
            GL.LoadPixelMatrix(0, 1, 1, 0);

            // Then clear & draw the texture to fill the entire RTT.
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            Graphics.DrawTexture(new Rect(0, 0, 1, 1), capture);

            // Update new texture
#if UNITY_2021_2_OR_NEWER
            capture.Reinitialize(size.x, size.y);
#else
            capture.Resize(size.x, size.y);
#endif
            capture.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);


            // Hacky workaround for issue with CaptureScreenshotAsTexture returning texture
            // in wrong colour space when using linear rendering
            if (m_UsingLinearRendering)
            {
                var pixels = capture.GetPixels();
                for (int i = 0; i < pixels.Length; ++i)
                {
//#if UNITY_EDITOR
                    pixels[i] = pixels[i].linear;
//#endif
                    pixels[i].a = 1f;
                }
                capture.SetPixels(pixels);
            }

            capture.Apply();

            // Compress the new texture
            if (compressed)
                capture.Compress(false);

            return capture;
        }

        #endregion


        #region SAVE FILE MANAGEMENT

        [Header("Location")]
        [SerializeField, Tooltip("")]
        private SavePathRoot m_SavePath = SavePathRoot.PersistantDataPath;
        [SerializeField, Tooltip("")]
        private string m_SaveSubFolder = "SaveFiles";

        const string k_Extension = "saveData";
        const string k_TypeStringQuick = "quick";
        const string k_TypeStringAuto = "auto";
        const string k_TypeStringManual = "manual";

        public static bool hasAvailableSaves
        {
            get
            {
                if (Instance == null)
                    return false;

                // Check initial count has been retrieved
                if (availableQuicksaves == null || availableAutosaves == null || availableManualSaves == null)
                    return false;

                // Check there's more than 1 save file available
                return (availableQuicksaves.Length > 0 || availableAutosaves.Length > 0 || availableManualSaves.Length > 0);
            }
        }

        public static FileInfo[] availableQuicksaves
        {
            get; private set;
        }

        public static FileInfo[] availableAutosaves
        {
            get; private set;
        }

        public static FileInfo[] availableManualSaves
        {
            get; private set;
        }

        public static FileInfo GetLatestSave(SaveGameTypeFilter filter)
        {
            if (Instance == null)
                return null;

            FileInfo result = null;
            if ((filter & SaveGameTypeFilter.Quicksave) != SaveGameTypeFilter.None && availableQuicksaves.Length > 0)
            {
                if (result == null || result.CreationTime < availableQuicksaves[0].CreationTime)
                    result = availableQuicksaves[0];
            }
            if ((filter & SaveGameTypeFilter.Autosave) != SaveGameTypeFilter.None && availableAutosaves.Length > 0)
            {
                if (result == null || result.CreationTime < availableAutosaves[0].CreationTime)
                    result = availableAutosaves[0];
            }
            if ((filter & SaveGameTypeFilter.Manual) != SaveGameTypeFilter.None && availableManualSaves.Length > 0)
            {
                if (result == null || result.CreationTime < availableManualSaves[0].CreationTime)
                    result = availableManualSaves[0];
            }
            return result;
        }

        public static FileInfo[] GetAvailableSaves(SaveGameTypeFilter filter)
        {
            if (Instance == null || filter == SaveGameTypeFilter.None)
                return new FileInfo[0];

            List<FileInfo> collected = new List<FileInfo>();
            if ((filter & SaveGameTypeFilter.Quicksave) != SaveGameTypeFilter.None)
                collected.AddRange(availableQuicksaves);
            if ((filter & SaveGameTypeFilter.Autosave) != SaveGameTypeFilter.None)
                collected.AddRange(availableAutosaves);
            if ((filter & SaveGameTypeFilter.Manual) != SaveGameTypeFilter.None)
                collected.AddRange(availableManualSaves);
            collected.Sort((FileInfo f1, FileInfo f2) => { return f2.CreationTime.CompareTo(f1.CreationTime); });
            return collected.ToArray();
        }

        static FileInfo[] CheckAvailableSaveFiles(SaveGameTypeFilter filter)
        {
            if (Instance != null)
            {
                DirectoryInfo directory = new DirectoryInfo(Instance.GetSaveFolder());
                if (directory != null && filter != SaveGameTypeFilter.None)
                {
                    FileInfo[] result = null;
                    switch (filter)
                    {
                        case SaveGameTypeFilter.All:
                            result = directory.GetFiles("*." + k_Extension);
                            break;
                        case SaveGameTypeFilter.Quicksave:
                            result = directory.GetFiles(string.Format("*_{0}.{1}", k_TypeStringQuick, k_Extension));
                            break;
                        case SaveGameTypeFilter.Autosave:
                            result = directory.GetFiles(string.Format("*_{0}.{1}", k_TypeStringAuto, k_Extension));
                            break;
                        case SaveGameTypeFilter.Manual:
                            result = directory.GetFiles(string.Format("*_{0}.{1}", k_TypeStringManual, k_Extension));
                            break;
                        default:
                            {
                                List<FileInfo> collected = new List<FileInfo>();
                                if ((filter & SaveGameTypeFilter.Quicksave) != SaveGameTypeFilter.None)
                                    collected.AddRange(directory.GetFiles(string.Format("*_{0}.{1}", k_TypeStringQuick, k_Extension)));
                                if ((filter & SaveGameTypeFilter.Autosave) != SaveGameTypeFilter.None)
                                    collected.AddRange(directory.GetFiles(string.Format("*_{0}.{1}", k_TypeStringAuto, k_Extension)));
                                if ((filter & SaveGameTypeFilter.Manual) != SaveGameTypeFilter.None)
                                    collected.AddRange(directory.GetFiles(string.Format("*_{0}.{1}", k_TypeStringManual, k_Extension)));
                                result = collected.ToArray();
                                break;
                            }
                    }
                    Array.Sort(result, (FileInfo f1, FileInfo f2) => { return f2.CreationTime.CompareTo(f1.CreationTime); });
                    return result;
                }
            }
            return new FileInfo[0];
        }

        IEnumerator RefreshAvailableSaves()
        {
            yield return null;

            // Get quicksaves (and trim excess)
            var files = new List<FileInfo>(CheckAvailableSaveFiles(SaveGameTypeFilter.Quicksave));
            files.Sort((FileInfo f1, FileInfo f2) => { return f2.CreationTime.CompareTo(f1.CreationTime); });
            if (files.Count > m_NumQuicksaves)
            {
                for (int i = files.Count; i > m_NumQuicksaves; --i)
                {
                    files[i - 1].Delete();
                    files.RemoveAt(i - 1);
                }
            }
            availableQuicksaves = files.ToArray();

            // Get autosaves (and trim excess)
            files = new List<FileInfo>(CheckAvailableSaveFiles(SaveGameTypeFilter.Autosave));
            files.Sort((FileInfo f1, FileInfo f2) => { return f2.CreationTime.CompareTo(f1.CreationTime); });
            if (files.Count > m_NumAutosaves)
            {
                for (int i = files.Count; i > m_NumAutosaves; --i)
                {
                    files[i - 1].Delete();
                    files.RemoveAt(i - 1);
                }
            }
            availableAutosaves = files.ToArray();

            // Get manualsaves
            availableManualSaves = CheckAvailableSaveFiles(SaveGameTypeFilter.Manual);
            Array.Sort(availableManualSaves, (FileInfo f1, FileInfo f2) => { return f2.CreationTime.CompareTo(f1.CreationTime); });
        }

        public void CheckSaveFolder()
        {
            string folder = GetSaveFolder();
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
        }

        public string GetSaveFolder()
        {
            switch (m_SavePath)
            {
                case SavePathRoot.PersistantDataPath:
                    if (string.IsNullOrEmpty(m_SaveSubFolder))
                        return Application.persistentDataPath + '/';
                    else
                        return string.Format("{0}/{1}/", Application.persistentDataPath, m_SaveSubFolder);
                case SavePathRoot.DataPath:
                    if (string.IsNullOrEmpty(m_SaveSubFolder))
                        return Application.dataPath + '/';
                    else
                        return string.Format("{0}/{1}/", Application.dataPath, m_SaveSubFolder);
                default:
                    return m_SaveSubFolder;
            }
        }

        string GetSavePath(DateTime time, SaveGameType type)
        {
            string typeString = string.Empty;
            switch (type)
            {
                case SaveGameType.Quicksave:
                    typeString = k_TypeStringQuick;
                    break;
                case SaveGameType.Autosave:
                    typeString = k_TypeStringAuto;
                    break;
                case SaveGameType.Manual:
                    typeString = k_TypeStringManual;
                    break;
            }

            return string.Format("{0}{1}{2}{3}{4}{5}{6}_{7}.{8}",
                GetSaveFolder(),
                time.Year, time.Month, time.Day,
                time.Hour, time.Minute, time.Second,
                typeString,
                k_Extension
                );
        }

        void ValidatePath()
        {
            // Check for empty string
            if (string.IsNullOrEmpty(m_SaveSubFolder))
                return;

#if UNITY_EDITOR
            var input = m_SaveSubFolder.Replace('\\', '/');
            var filtered = new List<char>(input.Length);
            var invalid = Path.GetInvalidPathChars();
            foreach (var c in input)
            {
                if (Array.IndexOf(invalid, c) == -1)
                    filtered.Add(c);
            }
            m_SaveSubFolder = new string(filtered.ToArray());
#endif
        }

        #endregion


        #region META-DATA

        private static LoadMetaDataJob m_LastMetaDataLoadJob = null;

        public static SaveFileMetaData[] LoadFileMetaData(SaveGameTypeFilter filter)
        {
            // Can't return meta-data while saving or loading
            if (Instance == null || inProgress)
                return new SaveFileMetaData[0];

            // Get the file infos and prep array
            var files = GetAvailableSaves(filter);
            var result = new SaveFileMetaData[files.Length];
            for (int i = 0; i < files.Length; ++i)
                result[i] = new SaveFileMetaData(files[i]);

            // Add async load job
            m_LastMetaDataLoadJob = new LoadMetaDataJob(Instance, result);
            Instance.AddJob(m_LastMetaDataLoadJob);

            return result;
        }

        public static void CancelLoadingFileMetaData()
        {
            if (m_LastMetaDataLoadJob != null)
            {
                if (!m_LastMetaDataLoadJob.completed)
                    m_LastMetaDataLoadJob.Abort();
                m_LastMetaDataLoadJob = null;
            }
        }

        class LoadMetaDataJob : AsyncSaveLoadJob
        {
            private Queue<SaveFileMetaData> m_Queue = new Queue<SaveFileMetaData>();
            private SaveFileMetaData m_CurrentMeta = null;

            public LoadMetaDataJob(SaveGameManager m, SaveFileMetaData[] meta) : base(m)
            {
                for (int i = 0; i < meta.Length; ++i)
                    m_Queue.Enqueue(meta[i]);
            }

            protected override void Process()
            {
                // Get the meta data to load this iteration
                m_CurrentMeta = null;
                lock (m_Queue)
                {
                    if (m_Queue.Count > 0)
                        m_CurrentMeta = m_Queue.Dequeue();
                }

                if (m_CurrentMeta == null)
                    completed = true;
                else
                {
                    using (var stream = m_CurrentMeta.saveFile.OpenRead())
                    {
                        if (manager.deserializer.ReadFromStream(stream))
                            WaitOnMainThreadTask(ReadProperties());
                    }

                    // Pause to allow for cleanup before loading the next
                    WaitOnMainThreadTask(Pause(0.5f));
                }
            }

            IEnumerator ReadProperties()
            {
                yield return null;

                var reader = manager.deserializer;
                reader.BeginDeserialization();
                if (reader.PushContext(SerializationContext.MetaData, 0))
                {
                    m_CurrentMeta.ReadProperties(reader);
                    reader.PopContext(SerializationContext.MetaData, 0);
                }
                reader.EndDeserialization();
            }

            IEnumerator Pause(float duration)
            {
                float timer = 0f;
                while (timer < duration)
                {
                    yield return null;
                    timer += Time.unscaledDeltaTime;
                }
            }

            public override void Abort()
            {
                lock (m_Queue)
                    m_Queue.Clear();
            }
        }

        #endregion


        #region SAVING

        public static event Action<SaveGameType> onSaveInProgess;
        public static event Action<SaveGameType> onSaveFailed;

        bool SaveGameInternal(SaveGameType type, string title, Action onComplete)
        {
            // Basic checks (since anyone can call this)
            if (inProgress || m_MainScene == null || (type == SaveGameType.Manual && !canManualSave))
            {
                if (onSaveFailed != null)
                    onSaveFailed(type);
                return false;
            }
            // Add the file write job
            AddJob(new SaveGameJob(Instance, title, type, onComplete));

            // Invoke save in-progress event
            if (onSaveInProgess != null)
                onSaveInProgess(type);

            return true;
        }


        class SaveGameJob : AsyncSaveLoadJob
        {
            private DateTime m_SaveTime = new DateTime();
            private SaveGameType m_SaveType = SaveGameType.Manual;
            private Action m_OnComplete = null;
            private string m_SaveTitle = string.Empty;
            private string m_SaveFilePath = string.Empty;

            public SaveGameJob(SaveGameManager m, string title, SaveGameType type, Action onComplete) : base(m)
            {
                m_SaveType = type;
                m_SaveTitle = title;
                m_SaveTime = DateTime.Now;
                m_OnComplete = onComplete;
            }

            protected override void Process()
            {
                WaitOnMainThreadTask(SaveGameCoroutine(m_SaveType, m_SaveTitle));

                // Write to stream
                using (var fstream = File.Create(m_SaveFilePath))
                {
                    manager.serializer.WriteToStream(fstream);
                }
                // Perform the onComplete
                if (m_OnComplete != null)
                    m_OnComplete();
                // Refresh available saves (also deletes excess)
                WaitOnMainThreadTask(manager.RefreshAvailableSaves());
                // Signal completed
                completed = true;
            }

            IEnumerator SaveGameCoroutine(SaveGameType type, string title)
            {
                yield return new WaitForEndOfFrame();

                m_SaveFilePath = manager.GetSavePath(m_SaveTime, m_SaveType);

                var serializer = manager.serializer;

                serializer.BeginSerialization();

                // Write metadata
                var meta = new SaveFileMetaData(title, type, m_SaveTime, manager.GetThumbnail(type));
                serializer.PushContext(SerializationContext.MetaData, 0);
                meta.WriteProperties(serializer);
                serializer.PopContext(SerializationContext.MetaData);

                // Save persistant (non-scene) data
                // ...

                // Save dont destroy on load scene contents
                // (requires adding a NeoSerializedScene based component to the SceneManager object)
                //var managerScene = gameObject.GetComponent<NeoSerializedScene>();
                //if (managerScene != null)
                //{
                //    serializer.PushContext(SerializationContext.Scene, -1);
                //    managerScene.WriteData(serializer);
                //    serializer.PopContext(SerializationContext.Scene);
                //}

                var scenes = manager.m_Scenes;

                // Prep for scene saves
                int sceneCount = scenes.Count;
                var m_FilteredPaths = new List<string>(sceneCount);
                var m_FilteredScenes = new List<NeoSerializedScene>(sceneCount);

                // Get main scene
                if (mainScene != null)
                {
                    //Debug.Log("Writing main scene path");
                    serializer.WriteValue(k_MainSceneKey, mainScene.scene.path);
                    m_FilteredScenes.Add(mainScene);
                }
                else
                {
                    serializer.WriteValue(k_MainSceneKey, string.Empty);
                }

                // Filter sub-scenes
                for (int i = 0; i < sceneCount; ++i)
                {
                    if (scenes[i] != mainScene)
                    {
                        m_FilteredScenes.Add(scenes[i]);
                        m_FilteredPaths.Add(scenes[i].scene.path);
                    }
                }

                // Write sub-scenes
                serializer.WriteValues(k_SubScenesKey, m_FilteredPaths);
                for (int i = 0; i < m_FilteredScenes.Count; ++i)
                {
                    //Debug.Log("Writing scene: " + m_FilteredScenes[i].scene.path + ", hash: " + NeoSerializationUtilities.StringToHash(m_FilteredScenes[i].scene.path));
                    serializer.PushContext(SerializationContext.Scene, NeoSerializationUtilities.StringToHash(m_FilteredScenes[i].scene.path));
                    m_FilteredScenes[i].WriteData(serializer);
                    serializer.PopContext(SerializationContext.Scene);
                }

                serializer.EndSerialization();

                yield return null;
            }

            public override void Abort()
            {
            }
        }

        #endregion


        #region LOADING

        private string m_LoadingScenePath = null;

        public static bool LoadGame(FileInfo saveFile)
        {
            if (Instance == null || inProgress)
                return false;

            // Read from stream
            var reader = Instance.deserializer;
            using (var fstream = saveFile.OpenRead())
            {
                reader.ReadFromStream(fstream);
            }

            reader.BeginDeserialization();

            // Load the main scene
            string mainScene;
            if (reader.TryReadValue(k_MainSceneKey, out mainScene, null))
            {
                // Add multi-scene loading later
                //string[] subScenes;
                //reader.TryReadValues(k_SubScenesKey, out subScenes, null);

                // Record scene name
                Instance.m_LoadingScenePath = mainScene;

                Instance.m_MainThreadBusy = true;
                NeoSceneManager.LoadScene(mainScene, OnCompleteLoad);
            }
            else
            {
                Debug.LogError("No main scene found");
                reader.EndDeserialization();
            }

            return true;
        }

        public static void NotifySceneLoaded(NeoSerializedScene scene)
        {
            RegisterScene(scene);

            // This method should be called by the NeoSerializedScene itself on Awake(),
            // And the NeoSerializedScene is set to execute Awake() before all other classes.
            var s = scene.scene;
            if (Instance.m_LoadingScenePath == s.path)
            {
                var reader = Instance.deserializer;
                if (reader.isDeserializing && reader.PushContext(SerializationContext.Scene, NeoSerializationUtilities.StringToHash(s.path)))
                {
                    try
                    {
                        var objects = s.GetRootGameObjects();
                        foreach (var obj in objects)
                        {
                            var sceneInfo = obj.GetComponent<SceneSaveInfo>();
                            if (sceneInfo != null)
                            {
                                sceneInfo.ReadData(reader);
                                break;
                            }
                        }
                    }
                    //catch (Exception e)
                    //{
                    //    Debug.LogError("Scene activation callback failed. There was an issue loading the scene: " + e.Message);
                    //}
                    finally
                    {
                        reader.PopContext(SerializationContext.Scene, NeoSerializationUtilities.StringToHash(s.path));
                    }
                }

                Instance.m_LoadingScenePath = null;
            }
        }

        static void OnCompleteLoad()
        {
            Instance.m_MainThreadBusy = false;

            Instance.deserializer.EndDeserialization();
        }

        #endregion


        #region SCENES

        private List<NeoSerializedScene> m_Scenes = new List<NeoSerializedScene>();
        private SceneSaveInfo m_MainScene = null;

        public static SceneSaveInfo mainScene
        {
            get
            {
                if (Instance != null)
                    return Instance.m_MainScene;
                else
                    return null;
            }
        }

        public static void RegisterScene(NeoSerializedScene scene)
        {
            if (Instance != null && scene != null)
            {
                if (scene.isMainScene)
                    Instance.m_MainScene = scene as SceneSaveInfo;
                Instance.m_Scenes.Add(scene);
                NeoSerializedObjectFactory.RegisterPrefabs(scene.registeredPrefabs);
                for (int i = 0; i < scene.registeredAssets.Length; ++i)
                {
                    var cast = scene.registeredAssets[i] as INeoSerializableAsset;
                    if (cast != null)
                        NeoSerializedObjectFactory.RegisterAsset(cast);
                }
            }
        }

        public static void UnregisterScene(NeoSerializedScene scene)
        {
            if (Instance != null && scene != null)
            {
                NeoSerializedObjectFactory.UnregisterPrefabs(scene.registeredPrefabs);
                for (int i = 0; i < scene.registeredAssets.Length; ++i)
                {
                    var cast = scene.registeredAssets[i] as INeoSerializableAsset;
                    if (cast != null)
                        NeoSerializedObjectFactory.UnregisterAsset(cast);
                }
                Instance.m_Scenes.Remove(scene);
                if (Instance.m_MainScene == scene)
                    Instance.m_MainScene = null;
            }
        }

        #endregion
    }
}