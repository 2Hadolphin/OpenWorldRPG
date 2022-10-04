using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheraBytes.BetterUi;
using System;
using Michsky.UI.Zone;
using Sirenix.OdinInspector;
using UnityEngine.Assertions;
using EnhancedUI.EnhancedScroller;
using EnhancedUI;
using UnityEngine.Events;
using UnityEngine.Scripting;
using Return;
using Return.Games;

namespace Return.UI
{
    [Serializable]
    public class MainSettingUIManager : SingletonMono<MainSettingUIManager>, IEnhancedScrollerDelegate, IStart
    {
        public const string Scroller = "Scrlloer";
        public const string Title = "Title";
        public const string GamePlay = "GamePlay";
        public const string Controls = "Controls";
        public const string Music = "Music";
        public const string Graphics = "Graphics";
        public enum SettingType
        {
            GamePlay,
            Control,
            Sound,
            Graphics
        }
        #region Field

        // ui call back field

        protected Transform Transform;

        [SerializeField]
        protected MainPage MainPage;

        [SerializeField]
        protected MainPanelManager MainPageManager;

        [SerializeField]
        protected MainPanelManager SettingPageManager;

        [SerializeField]
        public mUIPrefabs UIPrefabs;

        [BoxGroup(Scroller)]
        [SerializeField]
        public EnhancedScroller AScroller;

        Dictionary<EnhancedScroller, SmallList<SettingConfig>> ScrollCatch;

        [BoxGroup(Scroller)]
        [SerializeField]
        public EnhancedScroller BScroller;


        [BoxGroup(GamePlay)]
        [SerializeField]
        public RectTransform GamPlayField;

        [BoxGroup(Controls)]
        [SerializeField]
        public RectTransform ControlsField;

        [BoxGroup(Music)]
        [SerializeField]
        public RectTransform AudioField;

        [BoxGroup(Graphics)]
        [SerializeField]
        public RectTransform GraphicsField;

        #endregion

        bool IsOn;
        int Page = 0;

        #region Control


        [ShowInInspector]
        public Dictionary<Type, List<UIElemant>> _Controls = new();

        public virtual T GetControl<T>(Transform transform = null) where T : UIElemant
        {
            if (_Controls.TryGetValue(typeof(T), out var list))
            {
                if (SearchEnableControl(list, out T element))
                {
                    var rt = element.RectTransform;
                    var pos = rt.anchoredPosition;
                    rt.SetParent(transform, false);
                    //Debug.Log(string.Format("Apply control {0} to {1}", element.GetType(), transform.CharacterRoot.name));
                    //UnityEditor.EditorGUIUtility.PingObject(element);
                    //pos.height = 0;
                    rt.anchoredPosition = pos;
                    rt.localScale = Vector3.one;
                    element.gameObject.SetActive(true);
                    element.IsFree = false;
                    //element.ReleaseElement();
                    return element;
                }
            }
            else
            {
                list = new();
                _Controls.Add(typeof(T), list);
            }

            if (UIPrefabs.GetControlPrefabs.TryGetValue(typeof(T), out var prefab))
            {
                if (prefab is T newElement)
                {
                    var element = Instantiate(newElement, transform ? transform : Transform);

                    element.Release += Release;
                    element.IsFree = false;
                    list.Add(element);
                    return element;
                }
            }

            throw new KeyNotFoundException(typeof(T).ToString());
        }


        bool SearchEnableControl<T>(List<UIElemant> elemants, out T elemant) where T : UIElemant
        {
            foreach (T target in elemants)
            {
                if (target.IsFree)
                {
                    elemant = target;
                    return true;
                }
            }
            elemant = null;
            return false;
        }

        public virtual void Release(UIElemant elemant)
        {
            elemant.gameObject.SetActive(false);
            //elemant.transform.SetParent(ReadOnlyTransform,true);
            //if (elemant is SwitchManager @switch)
            //    SwitchToggles.Add(@switch);
        }

        #endregion

        protected override void Awake()
        {
            base.Awake();
            Transform = transform;
            MainPageManager.ActivatePanel += CheckPage;
            //SettingPageManager.ActivatePanel+=

            Routine.AddStartable(this);
        }

        void IStart.Initialize()
        {
            ScrollCatch = new(2);
            RegisterScroll(AScroller);
            RegisterScroll(BScroller);

            Init_Scroll();
            //SettingFieldScroller.ReloadData();
        }


        protected virtual void CheckPage(GameObject @object)
        {
            Debug.Log(@object);
            if (@object == SettingPageManager.gameObject)
            {
                Debug.Log(m_CurrentData);

                //on
                if (!IsOn)
                {
                    IsOn = true;
                    if (IsInvoking(nameof(ReleaseUI)))
                        CancelInvoke(nameof(ReleaseUI));
                }

                if (m_CurrentData != null && ScrollCatch[CurrentScroller] == m_CurrentData)
                    return;

                var sn = (int)SettingType.GamePlay;

                if (null != m_CurrentData)
                {
                    var index = 0;
                    foreach (var data in Configs)
                    {
                        if (data.Value == m_CurrentData)
                        {
                            sn = data.Key;
                            m_CurrentData = null;
                            SettingPageManager.PanelAnim(index);
                            break;
                        }
                        index++;
                    }
                }
                else
                    SettingPageManager.OpenFirstTab();

                SwitchData(sn);
            }
            else if (IsOn)
            {
                IsOn = false;
                // of get routine to delete page
                if (!IsInvoking(nameof(ReleaseUI)))
                    Invoke(nameof(ReleaseUI), 10);

            }
        }

        protected virtual void ReleaseUI()
        {
            //m_CurrentData = null;
            AScroller.ClearAll();
            BScroller.ClearAll();
            ScrollCatch[AScroller] = null;
            ScrollCatch[BScroller] = null;
            var list = new List<GameObject>();
            foreach (var data in _Controls)
            {
                var controls = data.Value;
                foreach (var control in controls)
                {
                    if (control)
                    {
                        var ob = control.gameObject;
                        ob.SetActive(false);
                        list.Add(ob);
                    }

                }
                controls.Clear();
            }

            StartCoroutine(CleanControls(list));
        }

        IEnumerator CleanControls(List<GameObject> list)
        {
            var length = list.Count;
            for (int i = 0; i < length; i++)
            {
                Destroy(list[i]);
                yield return null;
            }

            yield break;
        }

        #region Scroll


        protected bool IsAScroller;
        protected SmallList<SettingConfig> m_CurrentData;
        protected Dictionary<int, SmallList<SettingConfig>> Configs;


        public float TitleFieldHeight = 50;
        public float ButtonFieldHeight = 75;

        //[BoxGroup(Scroller)]
        //[SerializeField]
        //[ReadOnly]
        public EnhancedScroller CurrentScroller
        {
            set
            {
                IsAScroller = !IsAScroller;
                //Debug.Log("Using " + (IsAScroller ? AScroller.name : BScroller.name));
            }
            get
            {
                var scroller = IsAScroller ? AScroller : BScroller;
                if (!scroller.isActiveAndEnabled)
                    scroller.gameObject.SetActive(true);
                return scroller;
            }
        }


        void RegisterScroll(EnhancedScroller scroller)
        {
            ScrollCatch.Add(scroller, null);
            scroller.Delegate = this;
            scroller.cellViewVisibilityChanged = CellViewVisibilityChanged;
            scroller.cellViewWillRecycle = CellViewWillRecycle;
        }

        private void CellViewWillRecycle(EnhancedScrollerCellView cellView)
        {
            //Debug.LogError("Recycle "+cellView.CharacterRoot.name+ cellView.active);
        }

        /// <summary>
        /// Set controls
        /// </summary>
        private void CellViewVisibilityChanged(EnhancedScrollerCellView cellView)
        {
            //Debug.Log(
            //    cellView.CharacterRoot.name +
            //    (cellView.active ?
            //    " ApplyControl" :
            //    " ReleaseControl"
            //    ));

            // dispose control
            if (cellView is TitleField titleField)
                titleField.Release();
            else
                throw new KeyNotFoundException(cellView.GetType().ToString());

            if (!cellView.active)
                return;


            var scroller = CurrentScroller;

            UIElemant control = null;
            // get control data
            var configData = m_CurrentData[cellView.dataIndex];

            // determin what cell view to get based on the type of the data lines
            if (configData is TitleConfig)
            {
                //Debug.Log("generate TitleField : " + ConfigDatas[dataIndex].Title);
            }
            else if (cellView is ButtonField buttonField)
            {
                if (configData is ToggleConfig toggleConfig)
                {
                    // add toggle prefab
                    var toggle = GetControl<SwitchManager>(cellView.transform);
                    //UnityEditor.EditorGUIUtility.PingObject(toggle);

                    // set toggle value
                    toggle.SetValue(toggleConfig.Value);
                    // bind toggle to data link
                    toggle.OnValueChange.AddListener(new UnityAction<bool>(toggleConfig.SetValue));

                    // bind toggle with field interact
                    buttonField.Button.onClick.AddListener(new UnityAction(toggle.AnimateSwitch));
                    control = toggle;
                }
                else if (configData is HorEnumConfig selectConfig)
                {
                    var selector = GetControl<HorizontalSelector>(cellView.transform);
                    selector.AddOption(selectConfig.Values);
                    selector.Index = selectConfig.Value;
                    selector.onValueChanged.AddListener(new UnityAction<int>(selectConfig.SetValue));
                    buttonField.Button.onClick.AddListener(new UnityAction(selector.PreviousClick));
                    control = selector;
                }
                else
                    throw new KeyNotFoundException(configData.GetType().ToString());
            }
            else if (configData is SliderConfig sliderConfig)
            {
                var slider = GetControl<SliderManager>(cellView.transform);

                sliderConfig.Apply(slider.mainSlider);

                control = slider;
            }
            else if (configData is ButtonConfig buttonConfig)
            {
                var button = GetControl<ButtonControl>(cellView.transform);

                buttonConfig.Apply(button.Button, button.Title, 0);

                control = button;
            }
            else
                throw new KeyNotFoundException(configData.GetType().ToString());

            if (null != control)
            {
                //Debug.Log(cellView.active + " : " + control);
                if (cellView is TitleField controlField)
                {
                    // bind toggle with field activate
                    controlField.OnReleaseCellView -= control.ReleaseElement;
                    controlField.OnReleaseCellView += control.ReleaseElement;

                    controlField.ApplyControl -= ApplyControl;
                    controlField.ApplyControl += ApplyControl;
                }
                else
                    throw new KeyNotFoundException(configData.GetType().ToString());
            }

        }
        public static void AddConfigs(SettingType type, string title, params SettingConfig[] configs)
        {
            Instance.AddConfig(type, title, configs);
        }

        protected virtual void AddConfig(SettingType type, string title, params SettingConfig[] configs)
        {
            var sn = (int)type;
            if (!Configs.TryGetValue(sn, out var list))
            {
                list = new();
                Configs.Add(sn, list);
            }

            var length = list.Count;

            if (length > 0)
            {
                var insertSN = 0;
                //var match = System.StringComparison.OrdinalIgnoreCase;
                for (int i = 0; i < length; i++)
                {
                    if (list[i] is TitleConfig titleConfig && titleConfig.Title == title)
                    {
                        insertSN = i + 1;
                        break;
                    }
                }

                foreach (var config in configs)
                {
                    if (insertSN > 0 && config is TitleConfig newTitle && newTitle.Title == title)
                        continue;

                    list.Insert(config, insertSN);
                }
            }
            else
            {
                foreach (var config in configs)
                {
                    list.Add(config);
                }
            }
        }

        public virtual void ReloadData()
        {
            CurrentScroller.ReloadData();
        }


        public void SwitchData(int sn)
        {
            if (Configs.TryGetValue(sn, out var newConfigData))
            {
                if (m_CurrentData != newConfigData)
                {
                    m_CurrentData = newConfigData;
                    // switch scroller
                    CurrentScroller = null;
                    ScrollCatch[CurrentScroller] = newConfigData;
                    var rt = CurrentScroller.GetComponent<RectTransform>();
                    switch ((SettingType)sn)
                    {
                        case SettingType.GamePlay:
                            rt.SetParent(GamPlayField);
                            break;
                        case SettingType.Control:
                            rt.SetParent(ControlsField);
                            break;
                        case SettingType.Sound:
                            rt.SetParent(AudioField);
                            break;
                        case SettingType.Graphics:
                            rt.SetParent(GraphicsField);
                            break;
                        default:
                            throw new KeyNotFoundException(sn.ToString());
                    }

                    rt.anchoredPosition = Vector2.zero;
                    rt.localScale = Vector3.one;
                    ReloadData();
                }
                else
                    Debug.LogError("SameData");
            }
            else
                throw new KeyNotFoundException(((SettingType)sn).ToString());
        }

        /// <summary>
        /// UI callback
        /// </summary>
        /// <param name="sn"></param>
        public virtual void OrderCategory(int sn)
        {
            bool fwd = sn >= 0;

            sn = 0;
            foreach (var config in Configs)
            {
                if (config.Value == m_CurrentData)
                    break;
                sn++;
            }

            if (fwd)
            {
                sn++;
                SettingPageManager.NextPage();
            }
            else
            {
                sn--;
                SettingPageManager.PrevPage();
            }


            if (sn > Configs.Count || sn < 0)
            {
                Debug.LogError(sn);
                return;
            }


            var currentSN = 0;
            foreach (var config in Configs)
            {
                if (currentSN == sn)
                {
                    SwitchData(config.Key);
                    break;
                }
                currentSN++;
            }
        }

        /// <summary>
        /// Set exit dialogue and callback.
        /// </summary>
        public virtual void ExitSetting()
        {
            MainPage.ExitWindow.SetOption("Save Setting", "Revert Setting");
            MainPage.ExitWindow.SetCallback(MainPageManager.OpenFirstTab);
        }

        void Init_Scroll()
        {
            var types = Enum.GetValues(typeof(SettingType));

            Configs = new(types.Length);

            foreach (int sn in types)
            {
                var list = new SmallList<SettingConfig>();

                Configs.Add(sn, list);
            }

            #region Test

            //m_CurrentData = new();


            var gamplayconfigs = new SettingConfig[]
            {
            new TitleConfig() { Title = "Behaviour" },
            new ToggleConfig() { Title = "Show HUD", Value = true, Callback = Tutorials },
            new ToggleConfig() { Title = "Tutorials", Value = false, Callback = Tutorials },
            new ToggleConfig() { Title = "QQ", Value = true, Callback = Tutorials },
            new ToggleConfig() { Title = "pointer", Value = false, Callback = Tutorials },
            new ToggleConfig() { Title = "Music-Target", Value = true, Callback = Tutorials },
            new ToggleConfig() { Title = "Music-BB", Value = true, Callback = Tutorials },
            new ToggleConfig() { Title = "Music-CC", Value = true, Callback = Tutorials },
            new ToggleConfig() { Title = "Music-DD", Value = true, Callback = Tutorials },
            new ToggleConfig() { Title = "Music-EE", Value = true, Callback = Tutorials },
            };

            AddConfig(SettingType.GamePlay, "Behaviour", gamplayconfigs);


            var soundConfigs = new SettingConfig[]
            {
            new ToggleConfig() { Title = "Music", Value = true, Callback = Tutorials },
            new SliderConfig() { Title = "BB",MaxValue=10,MinValue=0 ,Value = 5, Callback = Tutorials },
            new ToggleConfig() { Title = "RR", Value = false, Callback = Tutorials },
            new SliderConfig() { Title = "MM", MaxValue = 5, MinValue = -5, Value = 3, Callback = Tutorials },
            new HorEnumConfig() { Title = "YY", Values = new[] { "AK","M416","M1"}, Callback = Tutorials },
            new ToggleConfig() { Title = "BackGroundMusic", Value = true, Callback = Tutorials },
            new ToggleConfig() { Title = "Music-Target", Value = true, Callback = Tutorials },
            new ToggleConfig() { Title = "Music-BB", Value = true, Callback = Tutorials },
            new ToggleConfig() { Title = "Music-CC", Value = true, Callback = Tutorials },
            new ToggleConfig() { Title = "Music-DD", Value = true, Callback = Tutorials },
            new ToggleConfig() { Title = "Music-EE", Value = true, Callback = Tutorials },
            };
            AddConfig(SettingType.Sound, "Music", soundConfigs);


            //var controlConfigs = new SettingConfig[]
            //{
            //    new ButtonConfig() { Title = "Movement", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback=BindKeyTest,BindingKey= "W-A-S-D" } } },
            //    new ButtonConfig() { Title = "Jump", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback=BindKeyTest,BindingKey= "Space" } } },
            //    new ButtonConfig() { Title = "Crouch", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback=BindKeyTest,BindingKey= "Left-Ctrl" } } },
            //    new ButtonConfig() { Title = "Climb", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback=BindKeyTest,BindingKey= "Left-Alt" } } },
            //    new ButtonConfig() { Title = "Sprint", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback=BindKeyTest,BindingKey= "Left-Shift" } } },
            //    new ButtonConfig() { Title = "IdleState", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback=BindKeyTest,BindingKey= "~" } } },
            //    new ButtonConfig() { Title = "Control-Target", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback=BindKeyTest,BindingKey= "Target" } } },
            //    new ButtonConfig() { Title = "Control-BB", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback=BindKeyTest,BindingKey= "BB" } } },
            //    new ButtonConfig() { Title = "Control-CC", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback=BindKeyTest,BindingKey= "CC" } } },
            //    new ButtonConfig() { Title = "Control-DD", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback=BindKeyTest,BindingKey= "DD" } } },
            //    new ButtonConfig() { Title = "Control-EE", Buttons=new []{ new ButtonConfig.ButtonConfigs {Callback=BindKeyTest,BindingKey= "EE" } } },
            //};
            //AddConfig(SettingType.Control, "Inputs Binding", controlConfigs);

            var grahpicsConfigs = new SettingConfig[]
            {
            new ToggleConfig() { Title = "Graphics", Value = true, Callback = Tutorials },
            new ToggleConfig() { Title = "VSync", Value = false, Callback = Tutorials },
            new ToggleConfig() { Title = "Shadow", Value = true, Callback = Tutorials },
            new HorEnumConfig() { Title = "DisplayMode", Values = new[] { "FullScreen","Window","Rect"}, Callback = Tutorials },
            new SliderConfig() { Title = "Blome", MaxValue = 10, MinValue = 0, Value = 3, Callback = Tutorials },
            new ToggleConfig() { Title = "Graphics-Target", Value = true, Callback = Tutorials },
            new ToggleConfig() { Title = "Graphics-BB", Value = true, Callback = Tutorials },
            new ToggleConfig() { Title = "Graphics-CC", Value = true, Callback = Tutorials },
            new ToggleConfig() { Title = "Graphics-DD", Value = true, Callback = Tutorials },
            new ToggleConfig() { Title = "Graphics-EE", Value = true, Callback = Tutorials },
            };
            AddConfig(SettingType.Graphics, "Graphics", grahpicsConfigs);

            //SwitchData((int)SettingType.GamePlay);
            #endregion
        }



        #endregion

        #region Test
        public virtual void Tutorials(int enable)
        {
            Debug.LogError(enable);
        }
        public virtual void Tutorials(float enable)
        {
            Debug.LogError(enable);
        }
        public virtual void Tutorials(bool enable)
        {
            Debug.LogError(enable);
        }

        public virtual void BindKeyTest()
        {
            //MainPage.BindingOption.Description.text = "Rebinding Key";
            MainPage.BindingOptionWindow.SetOption(null, "Cancel");
            MainPage.BindingOptionWindow.SetCallback(null, () => Debug.Log(MainPage.BindingOptionWindow.Description.text));
        }

        #endregion

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            if (scroller != CurrentScroller)
            {
                Debug.LogError(string.Format("Current : {0} \n Target : {1}", CurrentScroller, scroller));
                return 0;
            }

            var configs = ScrollCatch[scroller];

            if (null == configs)
                return default;
            else
                return configs.Count;

            return m_CurrentData.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            if (scroller != CurrentScroller)
                Debug.LogError(string.Format("Current : {0} \n Target : {1}", CurrentScroller, scroller));

            var configs = ScrollCatch[scroller];

            if (null == configs)
                return default;

            if (configs[dataIndex] is TitleConfig)
                return TitleFieldHeight;//(dataIndex % 2 == 0 ? 30f : 100f);
            else
                return ButtonFieldHeight;
        }


        /// <summary>
        /// Gets the cell to be displayed. You can have numerous cell BindingTypes, allowing variety in your list.
        /// Some examples of this would be headers, footers, and other grouping cells.
        /// </summary>
        /// <param name="scroller">The scroller requesting the cell</param>
        /// <param name="dataIndex">The index of the data that the scroller is requesting</param>
        /// <param name="cellIndex">The index of the list. This will likely be different from the dataIndex if the scroller is looping</param>
        /// <returns>The cell for the scroller to use</returns>
        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {

            if (scroller != CurrentScroller)
                Debug.LogError(string.Format("Current : {0} \n Target : {1}", CurrentScroller, scroller));

            var configs = ScrollCatch[scroller];
            Assert.IsNotNull(configs);
            var configData = configs[dataIndex];
            TitleField cellView;

            // determin what cell view to get based on the type of the data lines
            if (configData is TitleConfig)
            {
                cellView = scroller.GetCellView(UIPrefabs.TitleField) as TitleField;
            }
            else if (configData is IButtonField)
            {
                cellView = scroller.GetCellView(UIPrefabs.ButtonField) as TitleField;
            }
            else if (configData is INormalField)
            {
                cellView = scroller.GetCellView(UIPrefabs.NormalField) as TitleField;
            }
            else
                throw new KeyNotFoundException(configData.GetType().ToString());

            cellView.SetData(configData);

            return cellView;
        }

        protected virtual void ApplyControl(int dataIndex, int cellIndex)
        {
            //var cellView = CurrentScroller.GetCellViewAtDataIndex(dataIndex);
        }
    }
}