using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Return.Humanoid.Motion;
using Return.Motions;
using System.Linq;

namespace Return.Humanoid
{
    [Serializable]
    public abstract class MotionModulePreset : PresetDatabase
    {
        public const string DataPath = "Assets/Humanoid/MotionModule";

        [HideLabel]
        public MotionPriorityList Priorities;

        //[SerializeField]
        //public AnimationBehaviourBundle AnimationBundle;

        public abstract IHumanoidMotionModule Create(GameObject @object) ;

        #region Order

        [HorizontalGroup("Order")]
        [HideInInspector]
        public MotionOrder Order;



#if UNITY_EDITOR
        [HorizontalGroup("Order")]
        [ShowInInspector]
        public MotionOrder m_Order
        {
            get => Order;
            set
            {
                if (value && value != Order)
                {
                    if (Order)
                        Order.User.Remove(this);

                    Order = value;
                    Order.User.Add(this);
                }
                else if (null == value)
                {
                    if (Order)
                        Order.User.Remove(this);
                }
            }
        }

        [HorizontalGroup("Order")]
        [Button("CreateOrder")]
        void CreateMotionOrder()
        {
            m_Order = null;
            var newOrder = ScriptableObject.CreateInstance<MotionOrder>();
            var path = UnityEditor.AssetDatabase.GetAssetPath(this);

            path = path.Substring(0, path.LastIndexOf('/'));
            path = string.Format("{0}/MotionOrder_{1}.asset", path, this.name);
            UnityEditor.AssetDatabase.CreateAsset(newOrder, path);
            m_Order = UnityEditor.AssetDatabase.LoadAssetAtPath<MotionOrder>(path);
        }
#endif
        #endregion


    }
}

