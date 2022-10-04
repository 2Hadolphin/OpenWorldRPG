using UnityEngine;
using Return.mGUI;
using System;
using Return.Modular;
using EPOOutline;
using System.Collections.Generic;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine.Assertions;
using Return.Cameras;
using Object = UnityEngine.Object;

namespace Return.InteractSystem
{
    /// <summary>
    /// MonoModule to draw target outlines. **Selected **Locked
    /// </summary>
    public class SelectionVFX : BaseSelectionHandle<IVFXOption>,ICameraVolumeProvider
    {
        //Assets/Return/InteractSystem/Framework/Assistor/Resources/DefaultSelectionOutlines.asset
        const string DefaultConfigPath = "DefaultSelectionOutlines";

        [SerializeField]
        SelectionEffects m_OutlinePreset;
        public SelectionEffects OutLinePreset { get => m_OutlinePreset; set => m_OutlinePreset = value; }

        Dictionary<Object, Object> effectCache=new(2);

        protected virtual T GetEffect<T>(T prefab,Transform parent=null) where T : Object
        {
            if (!effectCache.TryGetValue(prefab, out var effect))
            {
                effect =
                    parent == null ?
                    Instantiate(prefab) :
                    Instantiate(prefab, parent);

                effectCache.Add(prefab, effect);
            }
            else
            {
                if (effect is Component comp)
                {
                    //comp.gameObject.SetActive(true);
                    comp.transform.SetParent(parent, false);
                    comp.transform.localScale = Vector3.one;
                }
            }

            return effect as T;
        }

        private void Awake()
        {
            if (OutLinePreset.IsNull())
                OutLinePreset = Resources.Load<SelectionEffects>(DefaultConfigPath);
        }

        void ICameraVolumeProvider.AddEffect(Camera camera)
        {
            var outliner = camera.InstanceIfNull<Outliner>();

            // load outliner data
        }

        void ICameraVolumeProvider.RemoveEffect(Camera camera)
        {
            if (camera.TryGetComponent<Outliner>(out var outliner))
                Destroy(outliner);
        }


        protected virtual void OnEnable()
        {
            CameraManager.AddVolume(this);

            // push as persistent visual effect stack
        }

        protected virtual void OnDisable()
        {
            CameraManager.RemoveVolume(this);
        }

        protected override bool AddTarget(InteractWrapper wrapper, out IVFXOption option)
        {
            option = null;

            if (wrapper.Interactable is not Component comp)
                return false;

            if (wrapper.Interactable is IInteractHandle interactable)
            {
                switch (interactable.HighLightMode)
                {
                    case HighLightMode.None:
                        return false;

                    case HighLightMode.Outline:
                        {
                            var outline = GetEffect(OutLinePreset.Selected, comp.transform);
                            outline.AddAllChildRenderersToRenderingList(comp.transform);
                            //outline.FrontParameters.DilateShift = 1f;
                            //outline.FrontParameters.BlurShift = 1f;
                            outline.CheckEnable();
                            option = new OutlineVFX() { Drawer = outline };

                            break;
                        }


                    case HighLightMode.ColliderBounds:

                        if (comp.TryGetComponent<Collider>(out var col))
                        {
                            var corner = GetEffect(OutLinePreset.Mark, comp.transform);
                            corner.SetTarget(col);

                            option = new HeightLightVFX() { Drawer = corner };
                        }
                        else
                        {
                            Debug.LogError($"Missing collider for vfx bounds : {comp}");
                            return false;
                        }
    
                        break;

                    case HighLightMode.RendererBounds:

                        if (comp.TryGetComponent<Renderer>(out var renderer))
                        {
                            var corner = GetEffect(OutLinePreset.Mark, comp.transform);
                            corner.SetTarget(renderer);

                            option = new HeightLightVFX() { Drawer = corner };
                        }
                        else
                        {
                            Debug.LogError($"Missing renderer for vfx bounds : {comp}");
                            return false;
                        }

                        break;

                    case HighLightMode.Custom:

                        if (comp is ICustomSelectionVFX selectable)
                        {
                            selectable.OnSelected(wrapper.Agent, m_handler);
                            return false;
                        }

                        break;
                }


            }
            
            // has effect
            return true;
        }


        protected override async UniTask RemoveTarget(IVFXOption value)
        {
            Assert.IsNotNull(value);

            value.Dispose();

            await UniTask.NextFrame();
        }

        void SetVFX(Outlinable outlinable)
        {
#if !EPO_DOTWEEN
            outlinable.enabled = true;
#else
            outlinable.FrontParameters.DOKill(true);
            outlinable.FrontParameters.DOColor(new Color(0, 1, 0, 1), 0.5f);
            outlinable.FrontParameters.DOBlurShift(1.0f, 0.5f).SetDelay(0.5f);
            outlinable.FrontParameters.DODilateShift(0.0f, 0.5f).SetDelay(0.5f);
            outlinable.FrontParameters.DOColor(new Color(1, 1, 0, 1), 0.5f).SetDelay(1.0f);
#endif
        }



        //        public void Init(HumanoidAgent_Exhibit reference)
        //        {
        //            lastRenderer = new Renderer[0];
        //            RegisterHandler();
        //            /*
        //            UILayout = Scripts.refO.scmaData.guiCentre.GetComponentInChildren<ItemLayout>();
        //            HDLayout = Scripts.refO.scmaData.guiCentre.GetComponentInChildren<HeaderGUI>();
        //*/
        //        }

        private void Register()
        {
            ItemPassEvent.PassItem += PassEvent_VFX; //ref to centre
        }

        private void PassEvent_VFX(object sender, ItemChainItemEventArgs e)
        {

        }


        public abstract class VFXOption<T> : IVFXOption where T:class
        {
            public T Drawer;

            public virtual void Dispose()
            {
                Drawer = null;
            }
        }

        public class OutlineVFX : VFXOption<Outlinable>
        {
            public override void Dispose()
            {
                //Destroy(Drawer.gameObject);
      
                //Drawer.gameObject.SetActive(false);
                Drawer.transform.parent = null;
                Drawer.transform.position = new Vector3(0, -5000, 0);
                Drawer.Clean();

                base.Dispose();
            }
        }

        public class HeightLightVFX : VFXOption<SelectionCorner>
        {

            public override void Dispose()
            {
                //Destroy(Drawer.gameObject);
                Drawer.transform.parent = null;
                Drawer.transform.position = new Vector3(0, -5000, 0);


                base.Dispose();
            }
        }

    }

    public interface IVFXOption : IDisposable
    {

    }

}