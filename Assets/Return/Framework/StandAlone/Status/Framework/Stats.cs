using System;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Return.Framework.Stats
{
    /// <summary>
    /// Mono stat logic handler.
    /// </summary>
    public partial class Stats : BaseComponent                          //, IAnimatorListener
    {
        /// <summary>List of Stats</summary>
        public List<Stat> stats = new ();

        /// <summary>List of Stats Converted to Dictionary</summary>
        public Dictionary<int, Stat> stats_D;

        /// <summary>Stored Stat to use the 'Pin' Methods</summary>
        public Stat PinnedStat;


        public void Initialize()
        {
            StopAllCoroutines();

            stats_D = new Dictionary<int, Stat>();

            foreach (var stat in stats)
            {
                if (stat.ID == null)
                {
                    Debug.LogError("One of the Stats has an Empty ID", gameObject);
                    break;
                }
                stat.InitializeStat(this);

                if (!stats_D.ContainsKey(stat.ID)) //Added by SkillManiacs 2020/10
                {
                    stats_D.Add(stat.ID, stat); //Convert them to Dictionary
                }
                else
                {
                    stats_D[stat.ID] = stat; //Replace it
                }
            }
        }

        private void Awake()
        {
            Initialize();
        }


        private void OnEnable()
        {
            foreach (var stat in stats_D)
            {
                if (stat.Value.ID == null)
                {
                    Debug.LogError("One of the Stats has an Empty ID", gameObject);
                    break;
                }
                stat.Value.InitializeStat(this);
            }
        }

        private void OnDisable() => StopAllCoroutines();





        /// <summary>Updates all Stats</summary>
        public virtual void Stats_Update()
        {
            foreach (var s in stats) s.UpdateStat();
        }

        /// <summary>Updates a stat logic by its Stat ID</summary>
        public virtual void Stats_Update(StatID iD) => Stats_Update(iD.ID);

        public virtual void Stats_Update(int iD) => Stat_Get(iD)?.UpdateStat();

        /// <summary> Reset a Stat to the Default Max Value</summary>
        public virtual void Stat_Reset_to_Max(StatID iD) => Stat_Get(iD)?.Reset_to_Max();

        /// <summary> Reset a Stat to the Default Min Value</summary>
        public virtual void Stat_Reset_to_Min(StatID iD) => Stat_Get(iD)?.Reset_to_Min();

        /// <summary>Disable a stat</summary>
        public virtual void Stat_Disable(StatID iD) => Stat_Get(iD)?.SetActive(false);

        /// <summary>Disable a stat Degeneration logic</summary>
        public virtual void Stat_Degenerate_Off(StatID ID) => Stat_Get(ID)?.SetDegeneration(false);

        /// <summary>Enable a stat Degeneration logic</summary>
        public virtual void Stat_Degenerate_On(StatID ID) => Stat_Get(ID)?.SetDegeneration(true);

        /// <summary>Disable a stat Regeneration logic</summary>
        public virtual void Stat_Regenerate_Off(StatID ID) => Stat_Get(ID)?.SetRegeneration(false);

        /// <summary>Enable a stat Regeneration logic</summary>
        public virtual void Stat_Regenerate_On(StatID ID) => Stat_Get(ID)?.SetRegeneration(true);


        #region Callbacks with StatID parameters
        /// <summary>Enable a stat</summary>
        public virtual void Stat_Enable(StatID iD) => Stat_Get(iD)?.SetActive(true);

        /// <summary>Set PinnedStat searching for a StatID</summary>
        public virtual void Stat_Pin(StatID ID) => Stat_Get(ID.ID);

        /// <summary>Find a Stat Using a StatID and Return if the Stat is on the List. Also Saves it to the PinnedStat</summary>
        public virtual Stat Stat_Get(StatID ID) => Stat_Get(ID.ID);

        // <summary>Set the Inmune Value of a Stat to true</summary>
        public virtual void Stat_Inmune_Activate(StatID ID) => Stat_Get(ID)?.SetInmune(true);

        /// <summary>Set the Inmune Value of a Stat to false</summary>
        public virtual void Stat_Inmune_Deactivate(StatID ID) => Stat_Get(ID)?.SetInmune(false);

        #endregion


        /// <summary>Set PinnedStat searching for a Stat Name</summary>
        public virtual void Stat_Pin(string name) => Stat_Get(name);

        /// <summary>Set PinnedStat searching for a int ID value</summary>
        public virtual void Stat_Pin(int ID) => Stat_Get(ID);


        /// <summary>Find a Stat Using its name for the ID and Return if the Stat is on the List. Also Saves it to the PinnedStat</summary>
        public virtual Stat Stat_Get(string name) => PinnedStat = stats.Find(item => item.Name == name);

        /// <summary>Find a Stat Using a int Value for the ID and Return if the Stat is on the List. Also Saves it to the PinnedStat</summary>
        public virtual Stat Stat_Get(int ID)
        {
            if (stats_D != null && stats_D.TryGetValue(ID, out PinnedStat))
                return PinnedStat;
            return null;
        }
        /// <summary>Find a Stat Using an IntVar and Return if the Stat is on the List. Also Saves it to the PinnedStat</summary>
        public virtual Stat Stat_Get(IntVar ID) => Stat_Get(ID.Value);

        public virtual void Stat_ModifyValue(StatID ID, float modifyvalue) => Stat_Get(ID)?.Modify(modifyvalue);
        public virtual void Stat_ModifyValue(int ID, float modifyvalue) => Stat_Get(ID)?.Modify(modifyvalue);
        public virtual void Stat_ModifyValue(string name, float modifyvalue) => Stat_Get(name)?.Modify(modifyvalue);

        public virtual void Stat_ModifyValue(StatID ID, float modifyvalue, StatOption modifyType) => Stat_Get(ID)?.Modify(modifyvalue, modifyType);
        public virtual void Stat_ModifyValue(string name, float modifyvalue, StatOption modifyType) => Stat_Get(name)?.Modify(modifyvalue, modifyType);

        /// <summary>Modify Stat Value instantly (Add/Remove to the Value)</summary>
        public virtual void Stat_Pin_ModifyValue(float value) => PinnedStat?.Modify(value);

        /// <summary>Modify Stat Value instantly (Add/Remove to the Value)</summary>
        public virtual void Stat_Pin_ModifyValue(FloatVar value) => PinnedStat?.Modify(value.Value);

        /// <summary>Modify Stat Value instantly (Add/Remove to the Value)</summary>
        public virtual void Stat_Pin_SetMult(float value) => PinnedStat?.SetMultiplier(value);

        /// <summary>Modify Stat Value instantly (Add/Remove to the Value)</summary>
        public virtual void Stat_Pin_SetMult(FloatVar value) => PinnedStat?.SetMultiplier(value.Value);

        /// <summary>Modify Stat Value in a X time period(Add/Remove to the Value)</summary>
        public virtual void Stat_Pin_ModifyValue(float value, float time) => PinnedStat?.Modify(value, time);

        /// <summary>Modify Stat Value in 1 second period(Add/Remove to the Value)</summary>
        public virtual void Stat_Pin_ModifyValue_1Sec(float value) => PinnedStat?.Modify(value, 1);

        /// <summary>Set  Stat Value to a fixed Value</summary>
        public virtual void Stat_Pin_SetValue(float value) => PinnedStat.SetValue(value);

        /// <summary>Modify the Pinned Stat MAX Value (Add or remove to the Max Value) </summary>
        public virtual void Stat_Pin_ModifyMaxValue(float value) => PinnedStat?.ModifyMAX(value);

        /// <summary>Set the Pinned Stat MAX Value </summary>
        public virtual void Stat_Pin_SetMaxValue(float value) => PinnedStat?.SetMAX(value);

        /// <summary> Enable/Disable the Pinned Stat Regeneration Rate </summary>
        public virtual void Stat_Pin_Modify_RegenRate(float value) => PinnedStat?.ModifyRegenRate(value);

        /// <summary> Enable/Disable the Pinned Stat Degeneration </summary>
        public virtual void Stat_Pin_Degenerate(bool value) => PinnedStat?.SetDegeneration(value);

        /// <summary> Enable/Disable the Pinned Stat Degeneration </summary>
        public virtual void Stat_Pin_SetInmune(bool value) => PinnedStat?.SetInmune(value);



        /// <summary>Enable/Disable the Pinned Stat Regeneration </summary>
        public virtual void Stat_Pin_Regenerate(bool value) => PinnedStat?.SetRegeneration(value);
        //  public virtual void _PinStatRegenerate(bool value) { Stat_Pin_Regenerate(value); }

        /// <summary> Enable/Disable the Pinned Stat</summary>
        public virtual void Stat_Pin_Enable(bool value) => PinnedStat?.SetActive(value);

        /// <summary>Modify the Pinned Stat value with a 'new Value',  'ticks' times , every 'timeBetweenTicks' seconds</summary>
        public virtual void Stat_Pin_ModifyValue(float newValue, int ticks, float timeBetweenTicks) => PinnedStat?.Modify(newValue, ticks, timeBetweenTicks);

        /// <summary> Clean the Pinned Stat from All Regeneration/Degeneration and Modify Tick Values </summary>
        public virtual void Stat_Pin_CleanCoroutines() => PinnedStat?.CleanRoutines();




        [Obsolete("Use Stat_Degenerate_Off instead")]
        /// <summary>Disable a stat Degeneration logic</summary>
        public virtual void DegenerateOff(StatID ID) => Stat_Degenerate_Off(ID);

        [Obsolete("Use Stat_Degenerate_On instead")]
        /// <summary>Enable a stat Degeneration logic</summary>
        public virtual void DegenerateOn(StatID ID) => Stat_Degenerate_On(ID);


    }

#if UNITY_EDITOR

#endif

}