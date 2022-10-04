//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using MalbersAnimations;
//using MalbersAnimations.Events;
//using MalbersAnimations.Scriptables;

//namespace Return.Framework.Stats
//{
//#if UNITY_EDITOR


//    public partial class Stats  // Editor Toolkit
//    {
//        [ContextMenu("Create/Stamina")]
//        private void ConnectStamina()
//        {
//            if (stats == null) stats = new List<Stat>();


//            var staminaID = MTools.GetInstance<StatID>("Stamina");


//            if (staminaID != null)
//            {
//                var staminaStat = Stat_Get(staminaID);

//                if (staminaStat == null)
//                {
//                    staminaStat = new Stat()
//                    {
//                        ID = staminaID,
//                        value = new FloatReference(100),
//                        ImmunityTime = new FloatReference(0.5f),
//                        regenerate = new BoolReference(true),
//                        RegenRate = new FloatReference(40),
//                        DegenRate = new FloatReference(20),
//                        RegenWaitTime = new FloatReference(2),
//                        Above = 15f,
//                        Below = 10f,
//                    };
//                    stats.Add(staminaStat);
//                }

//                //Connect to the Animal Controller in case it exist
//                var method = this.GetUnityAction<bool>("MAnimal", "UseSprint");

//                if (method != null)
//                {
//                    Debug.Log("medho" + method.ToString());
//                    UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(staminaStat.OnStatBelow, method, false);
//                    UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(staminaStat.OnStatAbove, method, true);
//                }

//                MEvent UIStamina = MTools.GetInstance<MEvent>("UI Stamina Stat");

//                if (UIStamina)
//                {
//                    UnityEditor.Events.UnityEventTools.AddPersistentListener(staminaStat.OnValueChangeNormalized, UIStamina.Invoke);
//                    UnityEditor.Events.UnityEventTools.AddPersistentListener(staminaStat.OnStatFull, UIStamina.Invoke);
//                }


//                var onSprintEnable = this.GetFieldClass<BoolEvent>("MAnimal", "OnSprintEnabled");

//                if (onSprintEnable != null)
//                {
//                    UnityEditor.Events.UnityEventTools.AddObjectPersistentListener<StatID>(onSprintEnable, Stat_Pin, staminaID);
//                    UnityEditor.Events.UnityEventTools.AddPersistentListener(onSprintEnable, Stat_Pin_Degenerate);
//                }

//                MTools.SetDirty(this);
//            }
//        }

//        [ContextMenu("Create/Health")]
//        void CreateHealth()
//        {
//            var health = MTools.GetInstance<StatID>("Health");

//            if (health != null)
//            {

//                var HealthStat = new Stat()
//                {
//                    ID = health,
//                    value = new FloatReference(100),
//                    DisableOnEmpty = new BoolReference(true),
//                    ImmunityTime = new FloatReference(0.1f)
//                };
//                stats.Add(HealthStat);


//                var deathID = MTools.GetInstance<StateID>("Death");

//                var method = this.GetUnityAction<StateID>("MAnimal", "State_Activate");

//                if (method != null) UnityEditor.Events.UnityEventTools.AddObjectPersistentListener<StateID>(HealthStat.OnStatEmpty, method, deathID);


//                MEvent UIHealth = MTools.GetInstance<MEvent>("UI Health Stat");

//                if (UIHealth)
//                {
//                    UnityEditor.Events.UnityEventTools.AddPersistentListener(HealthStat.OnValueChangeNormalized, UIHealth.Invoke);
//                    UnityEditor.Events.UnityEventTools.AddPersistentListener(HealthStat.OnStatFull, UIHealth.Invoke);
//                }

//                MTools.SetDirty(this);
//            }
//        }



//        [ContextMenu("Create/Mana")]
//        void CreateMana()
//        {
//            var Mana = MTools.GetInstance<StatID>("Mana");

//            if (Mana != null)
//            {

//                var HealthStat = new Stat()
//                {
//                    ID = Mana,
//                    value = new FloatReference(100),
//                    DisableOnEmpty = new BoolReference(true),
//                    ImmunityTime = new FloatReference(0),
//                    regenerate = new BoolReference(true),
//                    RegenWaitTime = new FloatReference(2),
//                    RegenRate = new FloatReference(10),
//                    DegenRate = new FloatReference(10)
//                };
//                stats.Add(HealthStat);


//                MEvent UIHealth = MTools.GetInstance<MEvent>("UI Mana Stat");

//                if (UIHealth)
//                {
//                    UnityEditor.Events.UnityEventTools.AddPersistentListener(HealthStat.OnValueChangeNormalized, UIHealth.Invoke);
//                    UnityEditor.Events.UnityEventTools.AddPersistentListener(HealthStat.OnStatFull, UIHealth.Invoke);
//                }

//                MTools.SetDirty(this);
//            }
//        }

//        private void Reset()
//        {
//            if (stats == null) stats = new List<Stat>();

//            CreateHealth();
//        }
//    }

//#endif

//}