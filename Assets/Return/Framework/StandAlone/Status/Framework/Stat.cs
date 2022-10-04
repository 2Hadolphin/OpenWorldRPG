using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;

namespace Return.Framework.Stats
{
    /// <summary>
    /// C# stat.
    /// </summary>
    [Serializable]
    public class Stat
    {
        #region Variables 

        [Tooltip("Enable/Disable the Stat. Disable Stats cannot be modified")]
        public bool active = true;
        [Tooltip("Key Idendifier for the Stat")]
        public StatID ID;
        [Tooltip("Current Value of the Stat")]
        public FloatReference value = new FloatReference(0);
        [Tooltip("Maximun Value of the Stat")]
        public FloatReference maxValue = new FloatReference(100);
        [Tooltip("Minimum Value of the Stat")]
        public FloatReference minValue = new FloatReference();
        [Tooltip("If the Stat is Empty it will be disabled to avoid future changes")]
        public BoolReference DisableOnEmpty = new BoolReference();

        /// <summary>Multiplier to modify the Stat value</summary>
        [SerializeField] 
        internal FloatReference multiplier = new (1);

        /// <summary>Can the Stat regenerate overtime</summary>
        [SerializeField] internal BoolReference regenerate = new BoolReference(false);

        /// <summary>Regeneration Rate. Change the Speed of the Regeneration</summary>
        public FloatReference RegenRate;

        /// <summary>Regeneration Rate. When the value is modified this will increase or decrease it over time.</summary>
        public FloatReference RegenWaitTime = new FloatReference(0);

        /// <summary>Regeneration Rate. When the value is modified this will increase or decrease it over time.</summary>
        public FloatReference DegenWaitTime = new FloatReference(0);

        /// <summary>Can the Stat degenerate overtime</summary>
        [SerializeField] internal BoolReference degenerate = new BoolReference(false);

        /// <summary>Degeneration Rate. Change the Speed of the Degeneration</summary>
        public FloatReference DegenRate;

        /// <summary>If greater than zero, the Stat cannot be modify until the inmune time have passed</summary>
        public FloatReference ImmunityTime;

        /// <summary>If the ResetStat funtion is called it will reset to Max or Low Value</summary>
        public ResetTo resetTo = ResetTo.MaxValue;

        /// <summary> Save the Last State of the Regeneration bool</summary>
        private bool regenerate_LastValue;

        /// <summary> Save the Last State of the Regeneration bool</summary>
        private bool degenerate_LastValue;

        /// <summary> Is the Stat Below the Below Value</summary>
        private bool isBelow = false;

        /// <summary> Is the Stat Above the Above Value</summary>
        private bool isAbove = false;
        #endregion

        #region Events
        public UnityEvent OnStatFull = new ();
        public UnityEvent OnStatEmpty = new ();
        public UnityEvent OnStat = new ();
        public float Below;
        public float Above;
        public UnityEvent OnStatBelow = new ();
        public UnityEvent OnStatAbove = new ();
        public FloatEvent OnValueChangeNormalized = new FloatEvent();
        public FloatEvent OnValueChange = new FloatEvent();
        public BoolEvent OnDegenerate = new BoolEvent();
        public BoolEvent OnRegenerate = new BoolEvent();
        public BoolEvent OnActive = new BoolEvent();
        #endregion

        #region Properties
        /// <summary>Is the Stat Enabled? when Disable no modification can be done. All current modification can't be stopped</summary>
        public bool Active
        {
            get => active;
            set
            {
                active = value;

                OnActive.Invoke(value);
                if (value)
                    StartRegeneration(); //If the Stat was activated start the regeneration
                else
                    StopRegeneration();
            }
        }

        public string Name
        {
            get
            {
                if (ID != null)
                {
                    return ID.name;
                }
                return string.Empty;
            }
        }

        /// <summary> Current value of the Stat</summary>
        public float Value
        {
            get => value;
            set => SetValue(value);
        }

        public bool IsFull => Value == MaxValue;
        public bool IsEmpty => Value == MinValue;

        /// <summary> Current Multiplier of the Stat</summary>
        public float Multiplier { get => multiplier.Value; set => multiplier.Value = value; }

        /// <summary>Returns the Normalized value of the Stat</summary>
        public float NormalizedValue => Value / MaxValue;

        /// <summary>If True: The Stat cannot be modify </summary>
        public bool IsInmune { get; set; }

        /// <summary>Maximum Value of the Stat</summary>
        public float MaxValue { get => maxValue.Value; set => maxValue.Value = value; }

        /// <summary>Minimun Value of the Stat </summary>
        public float MinValue { get => minValue.Value; set => minValue.Value = value; }

        /// <summary>Is the Stat Regenerating?</summary>
        public bool IsRegenerating { get; private set; }

        /// <summary>Is the Stat Degenerating?</summary>
        public bool IsDegenerating { get; private set; }

        [SerializeField] internal int EditorTabs = 0;

        /// <summary>Can the Stat Regenerate over time</summary>
        public bool Regenerate
        {
            get => regenerate.Value;
            set
            {
                regenerate.Value = value;
                regenerate_LastValue = regenerate;           //In case Regenerate is changed 
                OnRegenerate.Invoke(value);

                if (regenerate)
                {
                    degenerate.Value = false;     //Do not Degenerate if we are Regenerating
                    StopDegeneration();
                    StartRegeneration();
                }
                else
                {
                    degenerate.Value = degenerate_LastValue;   //If we are no longer Regenerating Start Degenerating again in case the Degenerate was true
                    StopRegeneration();
                    StartDegeneration();
                }
            }
        }

        /// <summary> Can the Stat Degenerate over time </summary>
        public bool Degenerate
        {
            get => degenerate.Value;
            set
            {
                degenerate.Value = value;
                degenerate_LastValue = degenerate;           //In case Regenerate is changed 
                OnDegenerate.Invoke(value);

                if (degenerate)
                {
                    regenerate.Value = false;     //Do not Regenerate if we are Degenerating
                    StartDegeneration();
                    StopRegeneration();
                }
                else
                {
                    regenerate.Value = regenerate_LastValue;   //If we are no longer Degenerating Start Regenerating again in case the Regenerate was true
                    StopDegeneration();
                    StartRegeneration();
                }
            }
        }

        #endregion

        [Obsolete]
        [NonSerialized] 
        WaitForSeconds InmuneWait;

        internal async void InitializeStat(Stats holder)
        {
            isAbove = isBelow = false;
            Owner = holder;                                     //Save the Monobehaviour to save coroutines

            if (value.Value >= Above) isAbove = true;           //This means that The Stat Value is over the Above value
            else if (value.Value <= Below) isBelow = true;      //This means that The Stat Value is under the Below value

            regenerate_LastValue = Regenerate;

            if (MaxValue < Value) MaxValue = Value;


            I_Regeneration = null;
            I_Degeneration = null;
            I_ModifyPerTicks = null;

            InmuneWait = new WaitForSeconds(ImmunityTime);

            if (Active)
            {
                StartRegeneration();
                StartDegeneration();
            }


            await UniTask.DelayFrame(3).AttachExternalCancellation(holder.GetCancellationTokenOnDestroy());

            ValueEvents();
        }

        internal void SetMultiplier(float value) => multiplier.Value = value;


        internal void ValueEvents()
        {


            OnValueChangeNormalized.Invoke(NormalizedValue);
            OnValueChange.Invoke(value);


            if (this.value == minValue.Value)
            {
                this.value.Value = minValue.Value;
                OnStatEmpty.Invoke();   //if the Value is 0 invoke Empty Stat

                if (DisableOnEmpty.Value)
                {
                    SetActive(false);
                    return;
                }

            }
            else if (this.value == maxValue.Value)
            {
                this.value.Value = maxValue.Value;
                OnStatFull.Invoke();    //if the Value is 0 invoke Empty Stat
            }


            if (this.value >= Above && !isAbove)
            {
                OnStatAbove.Invoke();
                isAbove = true;
                isBelow = false;
            }
            else if (this.value <= Below && !isBelow)
            {
                OnStatBelow.Invoke();
                isBelow = true;
                isAbove = false;
            }
        }

        internal void SetValue(float value)
        {
            var RealValue = Mathf.Clamp(value * Multiplier, MinValue, maxValue);

            if ((!Active) ||                                    //If the  Stat is not Active do nothing 
                (this.value.Value == RealValue)) return;        //If the values are equal do nothing. Avoid Stack Overflow

            this.value.Value = RealValue;

            ValueEvents();
        }

        /// <summary>Enable or Disable a Stat </summary>
        public void SetActive(bool value) => Active = value;
        public void SetRegeneration(bool value) => Regenerate = value;
        public void SetDegeneration(bool value) => Degenerate = value;
        public void SetInmune(bool value) => IsInmune = value;

        /// <summary>Adds or remove to the Stat Value </summary>
        public virtual void Modify(float newValue)
        {
            if (!IsInmune && Active)
            {
                Value += newValue;
                StartRegeneration();
                if (!Regenerate)
                    StartDegeneration();

                SetInmune();
            }
        }

        public virtual void UpdateStat()
        {
            SetValue(value);
            StartRegeneration();
            if (!Regenerate)
                StartDegeneration();
        }

        /// <summary>Adds or remove to the Stat Value</summary>
        public virtual void Modify(float newValue, float time)
        {
            if (!IsInmune && Active)
            {
                StopSlowModification();

                I_ModifySlow=C_SmoothChangeValue(newValue, time).ToUniTask(Owner).ToCoroutine();
                //Owner.StartCoroutine(out I_ModifySlow, C_SmoothChangeValue(newValue, time));

                SetInmune();
            }
        }

        /// <summary>  Modify the Stat value with a 'new Value',  'ticks' times , every 'timeBetweenTicks' seconds </summary>
        public virtual void Modify(float newValue, int ticks, float timeBetweenTicks)
        {
            if (!Active) 
                return;            //Ignore if the Stat is Disable

            StopCoroutine(I_ModifyPerTicks);
            I_ModifyPerTicks = C_ModifyTicksValue(newValue, ticks, timeBetweenTicks).ToUniTask(Owner).ToCoroutine();
            //Owner.StartCoroutine(out I_ModifyPerTicks, C_ModifyTicksValue(newValue, ticks, timeBetweenTicks));
        }

        /// <summary> Add or Remove Value the 'MaxValue' of the Stat </summary>
        public virtual void ModifyMAX(float newValue)
        {
            if (!Active) return;            //Ignore if the Stat is Disable
            MaxValue += newValue;
            StartRegeneration();
        }

        /// <summary>Sets the 'MaxValue' of the Stat </summary>
        public virtual void SetMAX(float newValue)
        {
            if (!Active) return;
            MaxValue = newValue;
            StartRegeneration();
        }


        /// <summary>Add or Remove Rate to the Regeneration Rate</summary>
        public virtual void ModifyRegenRate(float newValue)
        {
            if (!Active) return;            //Ignore if the Stat is Disable

            RegenRate.Value += newValue;
            StartRegeneration();
        }

        public virtual void SetRegenerationWait(float newValue)
        {
            if (!Active) return;            //Ignore if the Stat is Disable

            RegenWaitTime.Value = newValue;

            if (RegenWaitTime < 0) RegenWaitTime.Value = 0;
        }

        /// <summary>Set a new Regeneration Rate</summary>
        public virtual void SetRegenerationRate(float newValue)
        {
            if (!Active) return;            //Ignore if the Stat is Disable
            RegenRate.Value = newValue;
        }

        /// <summary> Reset the Stat to the Default Max Value</summary>
        public virtual void Reset() => Value = (resetTo == ResetTo.MaxValue) ? MaxValue : MinValue;

        /// <summary> Reset the Stat to the Default Min or Max Value</summary>
        public virtual void Reset_to_Max() => Value = MaxValue;

        /// <summary> Reset the Stat to the Default Min  Value</summary>
        public virtual void Reset_to_Min() => Value = MinValue;
        /// <summary>Clean all Coroutines</summary>
        internal void CleanRoutines()
        {
            StopDegeneration();
            StopRegeneration();
            StopTickDamage();
            StopSlowModification();
        }


        public virtual void RegenerateOverTime(float time)
        {
            if (time <= 0)
            {
                StartRegeneration();
            }
            else
            {
                C_RegenerateOverTime(time).ToUniTask(Owner);
            }
        }

        protected virtual void SetInmune()
        {
            if (ImmunityTime > 0)
            {
                StopCoroutine(I_IsInmune);
                //Owner.StartCoroutine(out I_IsInmune, C_InmuneTime());

                I_IsInmune = C_InmuneTime().ToUniTask(Owner).ToCoroutine();
            }
        }



        private void StopCoroutine(IEnumerator Cor)
        {
            if (Cor != null) Owner.StopCoroutine(Cor);
        }

        protected virtual void StartRegeneration()
        {
            StopRegeneration();

            if (RegenRate == 0 || !Regenerate) 
                return;   //Means if there's no Regeneration

            //Owner.StartCoroutine(out I_Regeneration, C_Regenerate());
            I_Regeneration=C_Regenerate().ToUniTask(Owner).ToCoroutine();
        }


        protected virtual void StartDegeneration()
        {
            StopDegeneration();
            if (DegenRate == 0 || !Degenerate) 
                return;  //Means there's no Degeneration

            //Owner.StartCoroutine(out I_Degeneration, C_Degenerate());

            I_Degeneration=C_Degenerate().ToUniTask(Owner).ToCoroutine();
        }

        protected virtual void StopRegeneration()
        {
            StopCoroutine(I_Regeneration);    //If there was a regenation active .... interrupt it

            I_Regeneration = null;
            IsRegenerating = false;
        }

        protected virtual void StopDegeneration()
        {
            StopCoroutine(I_Degeneration);    //if it was ALREADY Degenerating.. stop

            I_Degeneration = null;
            IsDegenerating = false;
        }

        protected virtual void StopTickDamage()
        {
            StopCoroutine(I_ModifyPerTicks);   //if it was ALREADY Degenerating.. stop...
            I_ModifyPerTicks = null;
        }

        protected virtual void StopSlowModification()
        {
            StopCoroutine(I_ModifySlow);       //If there was a regenation active .... interrupt it
            I_ModifySlow = null;
        }

        /// <summary>Modify the Stats on an animal </summary>
        public void Modify(float Value, StatOption modify)
        {
            switch (modify)
            {
                case StatOption.AddValue:
                    Modify(Value);
                    break;
                case StatOption.SetValue:
                    this.Value = Value;
                    break;
                case StatOption.SubstractValue:
                    Modify(-Value);
                    break;
                case StatOption.ModifyMaxValue:
                    ModifyMAX(Value);
                    break;
                case StatOption.SetMaxValue:
                    MaxValue = Value;
                    break;
                case StatOption.Degenerate:
                    DegenRate = Value;
                    Degenerate = true;
                    break;
                case StatOption.StopDegenerate:
                    DegenRate = Value;
                    Degenerate = false;
                    break;
                case StatOption.Regenerate:
                    Regenerate = true;
                    RegenRate = Value;
                    break;
                case StatOption.StopRegenerate:
                    Regenerate = false;
                    RegenRate = Value;
                    break;
                case StatOption.Reset:
                    Reset();
                    break;
                case StatOption.ReduceByPercent:
                    Modify(-(MaxValue * Value / 100));
                    break;
                case StatOption.IncreaseByPercent:
                    Modify(MaxValue * Value / 100);
                    break;
                case StatOption.Multiplier:
                    Multiplier = Value;
                    break;
                case StatOption.ResetToMax:
                    Reset_to_Max();
                    break;
                case StatOption.ResetToMin:
                    Reset_to_Min();
                    break;
                case StatOption.None:
                    break;
                default:
                    break;
            }
        }


        #region Coroutines
        /// <summary>
        ///  I need this to use coroutines in this class because it does not inherit from Monobehaviour, Also to Identify where is this Stat coming from
        /// </summary>
        public Stats Owner { get; private set; }
        private IEnumerator I_Regeneration;
        private IEnumerator I_Degeneration;
        private IEnumerator I_ModifyPerTicks;
        private IEnumerator I_ModifySlow;
        private IEnumerator I_IsInmune;


        protected IEnumerator C_RegenerateOverTime(float time)
        {
            float ReachValue = RegenRate > 0 ? MaxValue : MinValue;                                //Set to the default or 0
            bool Positive = RegenRate > 0;                                                          //Is the Regeneration Positive?
            float currentTime = Time.time;

            while (Value != ReachValue || currentTime > time)
            {
                Value += (RegenRate * Time.deltaTime);

                if (Positive && Value > MaxValue)
                {
                    Value = MaxValue;
                }
                else if (!Positive && Value < 0)
                {
                    Value = MinValue;
                }
                currentTime += Time.deltaTime;

                yield return null;
            }
            yield return null;
        }

        protected IEnumerator C_InmuneTime()
        {
            IsInmune = true;
            yield return InmuneWait;
            IsInmune = false;
        }

        protected IEnumerator C_Regenerate()
        {
            yield return null;

            if (RegenWaitTime > 0)
                yield return new WaitForSeconds(RegenWaitTime);          //Wait a time to regenerate

            IsRegenerating = true;



            while (Regenerate && Value < MaxValue)
            {
                Value += (RegenRate * Time.deltaTime);
                yield return null;
            }

            IsRegenerating = false;
            yield return null;
        }

        protected IEnumerator C_Degenerate()
        {
            yield return null;

            if (DegenWaitTime > 0)
                yield return new WaitForSeconds(DegenWaitTime);          //Wait a time to regenerate

            IsDegenerating = true;

            while (Degenerate && Value > MinValue)
            {
                Value -= (DegenRate * Time.deltaTime);
                yield return null;
            }
            IsDegenerating = false;
            yield return null;
        }

        protected IEnumerator C_ModifyTicksValue(float value, int Ticks, float time)
        {
            var WaitForTicks = new WaitForSeconds(time);

            for (int i = 0; i < Ticks; i++)
            {
                Value += value;
                if (Value <= MinValue)
                {
                    Value = MinValue;
                    break;
                }
                yield return WaitForTicks;
            }

            yield return null;

            StartRegeneration();
        }

        protected IEnumerator C_SmoothChangeValue(float newvalue, float time)
        {
            StopRegeneration();
            float currentTime = 0;
            float currentValue = Value;
            newvalue = Value + newvalue;

            yield return null;
            while (currentTime <= time)
            {

                Value = Mathf.Lerp(currentValue, newvalue, currentTime / time);
                currentTime += Time.deltaTime;

                yield return null;
            }
            Value = newvalue;

            yield return null;
            StartRegeneration();
        }
        #endregion

        public enum ResetTo
        {
            MinValue,
            MaxValue
        }
    }


}