using Return.Agents;
using UnityEngine;
using Return.Audios;
using Cysharp.Threading.Tasks;
using System;
using Return.InteractSystem;
using TNet;

namespace Return.Inventory
{
    /// <summary>
    /// Load storage content from disk data.
    /// </summary>
    public class Storage : BaseInventory, IStorage, IInteractable
    {
        #region IInteractSource

        [SerializeField]
        HighLightMode m_drawMode = HighLightMode.Outline;
        public virtual HighLightMode HighLightMode { get => m_drawMode; set => m_drawMode = value; }

        public void Interact(IAgent agent, object sender = null)
        {
            Debug.Log($"{agent} use inventory.");

            // show storage list 
            
            // play effect
            PlayEffect();
        }

        public void Cancel(IAgent agent, object sender = null)
        {
            Debug.Log($"{agent} close inventory.");

        }

        #endregion



        async UniTask PlayEffect()
        {
            //AudioManager.PlayOnceAt(m_Effects, transform.position);

            var preset = m_Bundle.Presets.Random(); //m_Bundle.Random();
            var clip = preset.Assets.Random().Asset;

            var source = AudioManager.GetTemplateSource(preset.Template);

            source.transform.Copy(transform);
            source.clip = clip;
            source.Play();

            var time = clip.length;

            await UniTask.Delay(TimeSpan.FromSeconds(time));
            //await UniTask.WaitUntil(()=>source.time.Difference(time) < 0.02f);

            AudioManager.ReturnTemplateSource(source);
        }

        public override bool Search(Func<IArchiveContent, bool> method, out object obj)
        {
            obj = null;
            return false;
        }

        public override bool CanStorage(object obj)
        {
            return false;
        }

        public override void Store(object obj)
        {
            
        }

        public override bool CanExtract(object obj)
        {
            return false;
        }

        public override void Extract(object obj)
        {
            
        }

        //[SerializeField]
        //AudioClip m_Effects;

        [SerializeField]
        AudioBundle m_Bundle;


    }
}