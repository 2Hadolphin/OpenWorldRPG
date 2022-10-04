using Return.UI;
using Michsky.UI;
using Michsky.MUIP;
using UnityEngine;
using Return.Scenes;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using UnityEngine.Assertions;
using System;

namespace Return.Games
{
    public class LoadSceneWindow : CustomWindow
    {
        [SerializeField,Required]
        ProgressBar m_progressBar;

        public ProgressBar ProgressBar { get => m_progressBar; set => m_progressBar = value; }


        [SerializeField]
        SceneData m_TargetScene;

        public SceneData TargetScene { get => m_TargetScene; set => m_TargetScene = value; }


        public virtual void LoadScene()
        {
            UniTask.Create(WaitSceneLoading).
                AttachExternalCancellation(this.GetCancellationTokenOnDestroy());
        }

        public virtual async UniTask WaitSceneLoading()
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

                var handle = TargetScene.LoadSceneAsync();
                
                Assert.IsFalse(handle == null);

                var update=Progress.Create<float>(OnProgressChanged);
                await handle.ToUniTask(update);


            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                enabled = false;
                Debug.Log("Load scene.");
            }
        }

        public void OnProgressChanged(float value)
        {
            value *= 100;
            Debug.Log($"Load scene progress {value}..");
            ProgressBar.ChangeValue(value);
        }
    }

}