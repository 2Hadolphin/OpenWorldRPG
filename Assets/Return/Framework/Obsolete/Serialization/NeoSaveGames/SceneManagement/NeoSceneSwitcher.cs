using UnityEngine;

namespace NeoSaveGames.SceneManagement
{
	public class NeoSceneSwitcher : MonoBehaviour
	{
        [Header("Target Scene")]
        [SerializeField, Tooltip("How to decide which scene to switch to. SceneName uses the scene name or path." +
            "SceneIndex uses the index in the build settings. NextSceneIndex gets the index from the current scene and uses the next.")]
        private SceneSwitchMode m_Mode = SceneSwitchMode.SceneName;
        [SerializeField, Tooltip("The scene to load. Make sure that the scene with this name is added to the project build properties.")]
        private string m_SceneName = string.Empty;
        [SerializeField, Tooltip("The scene to load. Make sure that this index matches the desired scene in your scene settings.")]
        private int m_SceneIndex = 0;

        [Header("Loading Scene")]
        [SerializeField, Tooltip("Load and display a custom \"loading screen\" scene or use the SaveGameManager default")]
        private LoadingSceneMode m_LoadingSceneMode = LoadingSceneMode.Default;
        [SerializeField, Tooltip("The loading screen scene to show. Make sure that the scene with this name is added to the project build properties.")]
        private string m_LoadingSceneName = "Loading";
        [SerializeField, Tooltip("The loading screen scene to show. Make sure that this index matches the desired scene in your scene settings.")]
        private int m_LoadingSceneIndex = 1;

        public enum SceneSwitchMode
        {
            SceneName,
            SceneIndex,
            NextSceneIndex,
            PreviousSceneIndex,
            ReloadCurrent
        }

        public enum LoadingSceneMode
        {
            Default,
            SceneName,
            SceneIndex
        }

        protected virtual void PreSceneSwitch()
        { }

        public void Switch ()
		{
            PreSceneSwitch();
            
            switch (m_Mode)
            {
                case SceneSwitchMode.SceneName:
                    switch (m_LoadingSceneMode)
                    {
                        case LoadingSceneMode.Default:
                            NeoSceneManager.LoadScene(m_SceneName);
                            break;
                        case LoadingSceneMode.SceneName:
                            NeoSceneManager.LoadScene(m_SceneName, m_LoadingSceneName);
                            break;
                        case LoadingSceneMode.SceneIndex:
                            NeoSceneManager.LoadScene(m_SceneName, m_LoadingSceneIndex);
                            break;
                    }
                    break;
                case SceneSwitchMode.SceneIndex:
                    switch (m_LoadingSceneMode)
                    {
                        case LoadingSceneMode.Default:
                            NeoSceneManager.LoadScene(m_SceneIndex);
                            break;
                        case LoadingSceneMode.SceneName:
                            NeoSceneManager.LoadScene(m_SceneIndex, m_LoadingSceneName);
                            break;
                        case LoadingSceneMode.SceneIndex:
                            NeoSceneManager.LoadScene(m_SceneIndex, m_LoadingSceneIndex);
                            break;
                    }
                    break;
                case SceneSwitchMode.NextSceneIndex:
                    {
                        int index = gameObject.scene.buildIndex + 1;
                        switch (m_LoadingSceneMode)
                        {
                            case LoadingSceneMode.Default:
                                NeoSceneManager.LoadScene(index);
                                break;
                            case LoadingSceneMode.SceneName:
                                NeoSceneManager.LoadScene(index, m_LoadingSceneName);
                                break;
                            case LoadingSceneMode.SceneIndex:
                                NeoSceneManager.LoadScene(index, m_LoadingSceneIndex);
                                break;
                        }
                    }
                    break;
                case SceneSwitchMode.PreviousSceneIndex:
                    {
                        int index = gameObject.scene.buildIndex - 1;
                        switch (m_LoadingSceneMode)
                        {
                            case LoadingSceneMode.Default:
                                NeoSceneManager.LoadScene(index);
                                break;
                            case LoadingSceneMode.SceneName:
                                NeoSceneManager.LoadScene(index, m_LoadingSceneName);
                                break;
                            case LoadingSceneMode.SceneIndex:
                                NeoSceneManager.LoadScene(index, m_LoadingSceneIndex);
                                break;
                        }
                    }
                    break;
                case SceneSwitchMode.ReloadCurrent:
                    {
                        int index = gameObject.scene.buildIndex;
                        switch (m_LoadingSceneMode)
                        {
                            case LoadingSceneMode.Default:
                                NeoSceneManager.LoadScene(index);
                                break;
                            case LoadingSceneMode.SceneName:
                                NeoSceneManager.LoadScene(index, m_LoadingSceneName);
                                break;
                            case LoadingSceneMode.SceneIndex:
                                NeoSceneManager.LoadScene(index, m_LoadingSceneIndex);
                                break;
                        }
                    }
                    break;
            }
        }
	}
}