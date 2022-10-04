using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Return;

namespace NeoSaveGames.SceneManagement
{
    [CreateAssetMenu(fileName = "FpsManager_NeoSceneManager")]
    [HelpURL("https://docs.neofps.com/manual/savegamesref-so-scenemanager.html")]
    public class NeoSceneManager : SingletonSO<NeoSceneManager>
    {
        private static RuntimeBehaviour s_ProxyGameObject = null;
        private static bool s_Busy = false;

        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        //static void LoadNeoSceneManager()
        //{
        //    var a=GetInstance(nameof(NeoSceneManager));
        //    a.name = "FpsManager_NeoSceneManager";
        //}

        public override void Initialize()
        {
            s_ProxyGameObject = GetBehaviourProxy<RuntimeBehaviour>();

        }

        public override bool IsValid()
        {
            return true;
        }




        [SerializeField, Tooltip("The loading screen scene to load by default if a custom one is not specified.")] 
        private int m_DefaultLoadingScreenIndex = -1;
        [SerializeField, Tooltip("The minimum amount of time the loading screen should be shown for. This prevents the loading screen from appearing for fractions of a second. Useful if you show tips.")]
        private float m_MinLoadScreenTime = 2f;

        [Header("Scene Load Events")]
        [SerializeField] private SceneLoadEvent m_OnSceneLoaded = new SceneLoadEvent();
        [SerializeField] private UnityEvent m_PreSceneActivation = new UnityEvent();
        [SerializeField] private UnityEvent m_OnSceneLoadFailed = new UnityEvent();

        enum LoadMode
        {
            Index,
            Name,
            Neither
        }

        [Serializable]
        public class SceneLoadEvent : UnityEvent<int> { }
        public class RuntimeBehaviour : MonoBehaviour { }
        
        public static int defaultLoadingScreenIndex
        {
            get
            {
                if (Instance != null)
                    return Instance.m_DefaultLoadingScreenIndex;
                else
                {
                    Debug.LogWarning("NeoSceneManager Instance not found");
                    return -1;
                }
            }
        }

        public static event UnityAction<int> onSceneLoaded
        {
            add
            {
                if (Instance != null)
                    Instance.m_OnSceneLoaded.AddListener(value);
            }
            remove
            {
                if (Instance != null)
                    Instance.m_OnSceneLoaded.RemoveListener(value);
            }
        }

        public static event UnityAction preSceneActivation
        {
            add
            {
                if (Instance != null)
                    Instance.m_PreSceneActivation.AddListener(value);
            }
            remove
            {
                if (Instance != null)
                    Instance.m_PreSceneActivation.RemoveListener(value);
            }
        }

        public static event UnityAction onSceneLoadFailed
        {
            add
            {
                if (Instance != null)
                    Instance.m_OnSceneLoadFailed.AddListener(value);
            }
            remove
            {
                if (Instance != null)
                    Instance.m_OnSceneLoadFailed.RemoveListener(value);
            }
        }

        private void OnValidate()
        {
            m_MinLoadScreenTime = Mathf.Clamp(m_MinLoadScreenTime, 0f, 60f);
        }

        public static bool isSceneValid(string sceneName)
        {
            return sceneName != null && Application.CanStreamedLevelBeLoaded(sceneName);
        }

        public static bool isSceneValid(int sceneIndex)
        {
            return Application.CanStreamedLevelBeLoaded(sceneIndex);
        }

        #region OVERRIDES

        public static void LoadScene(string sceneName)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(sceneName, -1, null, defaultLoadingScreenIndex, null, null));
        }

        public static void LoadScene(int sceneIndex)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(null, sceneIndex, null, defaultLoadingScreenIndex, null, null));
        }

        public static void LoadScene(string sceneName, string loadingSceneName)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(sceneName, -1, loadingSceneName, -1, null, null));
        }

        public static void LoadScene(int sceneIndex, int loadingIndex)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(null, sceneIndex, null, loadingIndex, null, null));
        }

        public static void LoadScene(string sceneName, int loadingIndex)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(sceneName, -1, null, loadingIndex, null, null));
        }

        public static void LoadScene(int sceneIndex, string loadingSceneName)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(null, sceneIndex, loadingSceneName, -1, null, null));
        }

        public static void LoadScene(string sceneName, Action onComplete)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(sceneName, -1, null, defaultLoadingScreenIndex, null, onComplete));
        }

        public static void LoadScene(int sceneIndex, Action onComplete)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(null, sceneIndex, null, defaultLoadingScreenIndex, null, onComplete));
        }

        public static void LoadScene(string sceneName, string loadingSceneName, Action onComplete)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(sceneName, -1, loadingSceneName, -1, null, onComplete));
        }

        public static void LoadScene(int sceneIndex, int loadingIndex, Action onComplete)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(null, sceneIndex, null, loadingIndex, null, onComplete));
        }

        public static void LoadScene(string sceneName, int loadingIndex, Action onComplete)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(sceneName, -1, null, loadingIndex, null, onComplete));
        }

        public static void LoadScene(int sceneIndex, string loadingSceneName, Action onComplete)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(null, sceneIndex, loadingSceneName, -1, null, onComplete));
        }

        public static void LoadScene(string sceneName, Action<Scene> activationCallback)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(sceneName, -1, null, defaultLoadingScreenIndex, activationCallback, null));
        }

        public static void LoadScene(int sceneIndex, Action<Scene> activationCallback)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(null, sceneIndex, null, defaultLoadingScreenIndex, activationCallback, null));
        }

        public static void LoadScene(string sceneName, string loadingSceneName, Action<Scene> activationCallback)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(sceneName, -1, loadingSceneName, -1, activationCallback, null));
        }

        public static void LoadScene(int sceneIndex, int loadingIndex, Action<Scene> activationCallback)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(null, sceneIndex, null, loadingIndex, activationCallback, null));
        }

        public static void LoadScene(string sceneName, int loadingIndex, Action<Scene> activationCallback)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(sceneName, -1, null, loadingIndex, activationCallback, null));
        }

        public static void LoadScene(int sceneIndex, string loadingSceneName, Action<Scene> activationCallback)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(null, sceneIndex, loadingSceneName, -1, activationCallback, null));
        }

        public static void LoadScene(string sceneName, Action<Scene> activationCallback, Action onComplete)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(sceneName, -1, null, defaultLoadingScreenIndex, activationCallback, onComplete));
        }

        public static void LoadScene(int sceneIndex, Action<Scene> activationCallback, Action onComplete)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(null, sceneIndex, null, defaultLoadingScreenIndex, activationCallback, onComplete));
        }

        public static void LoadScene(string sceneName, string loadingSceneName, Action<Scene> activationCallback, Action onComplete)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(sceneName, -1, loadingSceneName, -1, activationCallback, onComplete));
        }

        public static void LoadScene(int sceneIndex, int loadingIndex, Action<Scene> activationCallback, Action onComplete)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(null, sceneIndex, null, loadingIndex, activationCallback, onComplete));
        }

        public static void LoadScene(string sceneName, int loadingIndex, Action<Scene> activationCallback, Action onComplete)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(sceneName, -1, null, loadingIndex, activationCallback, onComplete));
        }

        public static void LoadScene(int sceneIndex, string loadingSceneName, Action<Scene> activationCallback, Action onComplete)
        {
            if (Instance != null && !s_Busy)
                s_ProxyGameObject.StartCoroutine(LoadSceneInternal(null, sceneIndex, loadingSceneName, -1, activationCallback, onComplete));
        }

        #endregion
        
        static IEnumerator LoadSceneInternal(string sceneName, int sceneIndex, string loadingSceneName, int loadingSceneIndex, Action<Scene> activationCallback, Action onComplete)
        {
            s_Busy = true;

            // Check how to load the scene
            LoadMode sceneLoadMode = LoadMode.Neither;
            if (isSceneValid(sceneIndex))
                sceneLoadMode = LoadMode.Index;
            else
            {
                if (isSceneValid(sceneName))
                    sceneLoadMode = LoadMode.Name;
            }

            if (sceneLoadMode == LoadMode.Neither)
            {
                Debug.LogError(string.Format("Attempting to load invalid scene. Check it exists and is added to the build options.", sceneName));
                Instance.m_OnSceneLoadFailed.Invoke();
                if (onComplete != null)
                    onComplete();
            }
            else
            {
                // Check how to load the loading overlay scene
                LoadMode overlayLoadMode = LoadMode.Neither;
                if (isSceneValid(loadingSceneIndex))
                    overlayLoadMode = LoadMode.Index;
                else
                {
                    if (isSceneValid(loadingSceneName))
                        overlayLoadMode = LoadMode.Name;
                }

                if (overlayLoadMode == LoadMode.Neither)
                {
                    Debug.LogError("\"Loading\" scene is not valid. Reverting to standard SceneManager.");

                    AsyncOperation op = null;
                    if (sceneLoadMode == LoadMode.Index)
                        op = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);
                    else
                        op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

                    while (!op.isDone)
                        yield return null;

                    // Signal completion
                    if (onComplete != null)
                        onComplete();
                }
                else
                {
                    var startTime = Time.realtimeSinceStartup;

                    AsyncOperation op = null;
                    Scene loadingScene = new Scene();

                    // Open "loading" scene (immediate)
                    if (overlayLoadMode == LoadMode.Index)
                    {
                        op = SceneManager.LoadSceneAsync(loadingSceneIndex, LoadSceneMode.Single);
                        loadingScene = SceneManager.GetSceneByBuildIndex(loadingSceneIndex);
                    }
                    else
                    {
                        op = SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Single);
                        loadingScene = SceneManager.GetSceneByName(loadingSceneName);
                    }
                    while (!op.isDone)
                        yield return null;

                    // Load the main scene in the background (async)
                    Scene scene = new Scene();
                    if (sceneLoadMode == LoadMode.Index)
                    {
                        op = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
                        scene = SceneManager.GetSceneByBuildIndex(sceneIndex);
                    }
                    else
                    {
                        op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                        //sceneName = sceneName.Substring(7, sceneName.Length - 13);
                        scene = SceneManager.GetSceneByName(sceneName);
                        if (!scene.IsValid())
                            scene = SceneManager.GetSceneByPath(sceneName);
                    }

                    SceneManager.sceneLoaded += OnSceneLoaded;

                    // Prevent activation and wait for pre-activation to complete
                    op.allowSceneActivation = false;
                    while (op.progress < 0.9f)
                        yield return null;

                    // Wait for minimum load time
                    while (Time.realtimeSinceStartup < startTime + Instance.m_MinLoadScreenTime)
                        yield return null;

                    // Invoke pre-activation event
                    Instance.m_PreSceneActivation.Invoke();

                    // Allow activation and wait for complete
                    op.allowSceneActivation = true;
                    while (!op.isDone)
                        yield return null;

                    // Set the scene to active and signal activation
                    SceneManager.SetActiveScene(scene);
                    if (activationCallback != null)
                        activationCallback(scene);

#if UNITY_2019_3_OR_NEWER
                    // Rebuild light-probes
                    LightProbes.Tetrahedralize();
#endif

                    // Unload the "loading" scene
                    op = SceneManager.UnloadSceneAsync(loadingScene.buildIndex);
                    if (op == null)
                    {
                        Debug.LogError("Can't unload \"Loading\" scene, because the main scene failed to load.");

                        // Invoke failed event
                        Instance.m_OnSceneLoadFailed.Invoke();
                    }
                    else
                    {
                        while (!op.isDone)
                            yield return null;

                        // Invoke succeeded event
                        Instance.m_OnSceneLoaded.Invoke(scene.buildIndex);
                    }

                    // Signal completion
                    if (onComplete != null)
                        onComplete();
                }
            }

            s_Busy = false;
        }

        static void OnSceneLoaded(Scene s, LoadSceneMode mode)
        {
            SceneManager.SetActiveScene(s);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }


        
    }


}