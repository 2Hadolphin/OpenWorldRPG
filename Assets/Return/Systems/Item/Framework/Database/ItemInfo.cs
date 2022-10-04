using UnityEngine;
using System;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Linq;
using Return.Database;
using System.Collections.Generic;
using Return.Inventory;

namespace Return.Items
{
    /// <summary>
    /// m_items data in common use **MonoModule **Volume **Weight **HandPose **Sprite **Model **m_category
    /// </summary>
    [Serializable]
    public abstract class ItemInfo : BasicInfo, ISerializationCallbackReceiver//, IValue<ItemPreset>
    {
        #region Info

        //[BoxGroup("Split/Info")]
        [TextArea(4, 14)]
        public string Infomation;

        //[HorizontalGroup("Split", 0.5f, MarginLeft = 5, LabelWidth = 130)]
        //[BoxGroup("Split/Info/Notes")]
        [TextArea(4, 9)]
        public string Notes;


        //[BoxGroup(STATS_BOX_GROUP)]
        //[HorizontalGroup(LEFT_VERTICAL_GROUP + "/General Settings/Split", 55, LabelWidth = 67)]
        [SerializeField]
        InveonoryIcon m_Icon;
        public InveonoryIcon Icon { get => m_Icon; set => m_Icon = value; }

        /// <summary>
        /// Items weight.
        /// </summary>
        [SerializeField]
        [Tooltip("The item weight.")]
        float m_weight=1f;
        public float Weight { get => m_weight; set => m_weight = value; }

        [ListDrawerSettings(Expanded = true, NumberOfItemsPerPage = 5, DraggableItems = true)]
        //[BoxGroup(STATS_BOX_GROUP)]
        [ShowInInspector]
        public HashSet<ItemCategory> Categories = new();

        //[VerticalGroup(GENERAL_SETTINGS_VERTICAL_GROUP)]
        [ValueDropdown("SupportedItemTypes")]
        [ValidateInput("IsSupportedType")]
        Category m_Category;
        public Category Category { get => m_Category; set => m_Category = value; }


        #endregion

        #region Instance



        [SerializeField, AssetsOnly, JsonIgnore]
        //[VerticalGroup(GENERAL_SETTINGS_VERTICAL_GROUP)]
        GameObject m_Model;
        public GameObject Model { get => m_Model; set => m_Model = value; }

        [SerializeField, AssetsOnly, JsonIgnore]
        //[VerticalGroup(GENERAL_SETTINGS_VERTICAL_GROUP)]
        GameObject m_ShowCase;
        public GameObject ShowCase { get => m_ShowCase; set => m_ShowCase = value; }

        [SerializeField, AssetsOnly, JsonIgnore]
        //[VerticalGroup(GENERAL_SETTINGS_VERTICAL_GROUP)]
        GameObject m_Prefab;
        public GameObject Prefab { get => m_Prefab; set => m_Prefab = value; }

        #endregion


        #region Modules

        [Space]
        [PropertyTooltip("Items Modules")]
        [JsonIgnore]
        public ConfigurableItemModulePreset[] Modules;

        public bool TryGetModulePreset<T>(out T result)
        {
            foreach (var module in Modules)
            {
                if(module is T target)
                {
                    result = target;
                    return true;
                }
            }
            result = default;
            return false;
        }

        #endregion

   

        #region Config
        [JsonIgnore]
        [Tooltip("m_items offset in handles")]
        public ItemPostureAdjustData HandPose;
        #endregion



        #region Editor
        protected override void OnAfterDeserialize()
        {
            SpriteBinding.SetDirty();
        }

        protected override void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            Test();
#endif
        }

#if UNITY_EDITOR
        public void Test()
        {
            if (sprite != null)
                SpriteBinding = new ResourcesBinding(ResourcesBinding.Editor_GetObjectAddress(sprite));
            else
                SpriteBinding = ResourcesBinding.empty;
        }
#endif
        #endregion





        #region Resource

        [HideLabel, PreviewField(55)]
        [JsonIgnore]
        //[VerticalGroup(LEFT_VERTICAL_GROUP)]
        [OnValueChanged(nameof(Test), InvokeOnInitialize = false)]
        public Sprite sprite;

        [Obsolete]
        [JsonIgnore]
        public Sprite Sprite
        {
            get => SpriteBinding.GetValue<Sprite>();
#if UNITY_EDITOR
            set => Debug.Log(value);//SpriteBinding = new ResourcesBinding(ResourcesBinding.Editor_GetObjectAddress(value));
#else
                
#endif
        }

        [Obsolete]
        [SerializeField]
        [ReadOnly]
        ResourcesBinding SpriteBinding;
        #endregion


        private bool IsSupportedType(Category type)
        {
            if (type is ItemCategory itemCategory)
                return Categories.Contains(itemCategory);
            else
                return false;
        }
    }
}