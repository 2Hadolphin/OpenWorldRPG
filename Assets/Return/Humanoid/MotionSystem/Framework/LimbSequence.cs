using System.Collections.Generic;
using UnityEngine.Assertions;
using System;
using UnityEngine;
using Return;
using Return.Creature;
using Sirenix.OdinInspector;
using Return.Motions;

namespace Return.Humanoid
{

    /// <summary>
    /// CanMotion => ConfirmMotion =>
    /// </summary>
    [Serializable]
    public class LimbSequence : NullCheck, ISequence
    {
        public event Action<int,bool, Limb> Verify;
        public bool DisableMotion = false;
        public LimbSequence(Limb limb)
        {
            Limb = limb;
        }

        public readonly Limb Limb;

        //protected HashSet<MotionModule> Modules = new HashSet<MotionModule>();

        // **hold gun and press button **
        /// <summary>
        /// Activated motion modules which execute via this limb
        /// </summary>
        [ShowInInspector,ReadOnly]
        protected HashSet<IMotionModule_Huamnoid> Current = new(3);

        /// <summary>
        /// Motion modules in queue
        /// </summary>
        [ShowInInspector, ReadOnly]
        protected List<IMotionModule_Huamnoid> Queue = new();
        
        //public IEnumerator<MotionModule> ActivateModules => Modules.GetEnumerator();

        public bool IsFree => Current.Count.Equals(default);

        class WaitMotionConfirm
        {
            /// <summary>
            /// List of motion newModule to deal with.
            /// </summary>
            public List<IMotionModule_Huamnoid> Remove=new ();

            public IMotionModule_Huamnoid WaitModule;

            public void Clear()
            {
                Remove.Clear();
                WaitModule = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        readonly WaitMotionConfirm WaitConfirm=new ();


        /// <summary>
        /// CheckAdd whether  this limb sequence can motion 
        /// </summary>
        /// <param name="newModule">newModule to control</param>
        /// <param name="waitConfirm">set as true if newModule need to check qualify with other limbs</param>
        /// <returns></returns>
        public bool CanMotion(IMotionModule_Huamnoid newModule,bool waitConfirm=false)
        {
            if (DisableMotion)
                return false;

            if(waitConfirm)
                WaitConfirm.Clear();

            var length = Current.Count;

            // non activated motion
            if (length.Equals(default))
            {
                if (waitConfirm)
                    WaitConfirm.WaitModule = newModule;
                else
                    AddCurrent(newModule);
                return true;
            }
            //  has activated motion
            else
            {

                foreach (var curModule in Current)
                {
                    if (newModule.Equals(curModule))
                        return true;

                    if (curModule.HasMotion)
                    {
                        var order = curModule.CompareTo(newModule);

                        if (order < 0) // priority of new module is lower than current motion
                        {
                            // set waiting queue for callback after current motion finish
                            Enqueue(newModule);

                            return false;
                        }
                        else if (order > 0) // new module has heigher priority than current motion
                        {
                            WaitConfirm.Remove.Add(curModule);
                        }
                        else
                        {
                            WaitConfirm.Remove.Add(curModule);
                        }
                    }
                    else
                        Debug.LogError($"{curModule} isn't doing motion, will not eneque.");
                }

                //  cache motion if require sequenc group valid
                if (waitConfirm)
                    WaitConfirm.WaitModule = newModule;
                //  push sequenc immediately
                else
                {
                    if (WaitConfirm.Remove.Count > 0)
                        foreach (var removeModule in WaitConfirm.Remove)
                        {
                            RemoveCurrent(removeModule);
                            //removeModule.InterruptMotion(Limb);
                        }

                    AddCurrent(newModule);
                }
                return true;
            }
        }

        /// <summary>
        /// Invoke after CanPlay to push motion into current user.
        /// </summary>
        public virtual void ConfirmMotion(IMotionModule_Huamnoid module)
        {
            // non motion execute clean confirms
            if (module == null)
            {
                WaitConfirm.Clear();
                return;
            }

            if (module != WaitConfirm.WaitModule)
            {
                Debug.LogError($"Waitting motion confirm not match input newModule waitting : [{WaitConfirm.WaitModule?.GetData.name}] confirm : [{module.GetData.name}].");
                return;
            }

            Debug.LogError($"Sequence confirm new newModule {module.GetData.name} with {Limb}.");

            AddCurrent(module);

            try
            {
                if (WaitConfirm.Remove.Count > 0)
                {
                    var remove = WaitConfirm.Remove;
                    var length = remove.Count;

                    for (int i = 0; i < length; i++)
                    {
                        var removeModule = remove[i];

                        if (removeModule == null)
                            continue;

                        if (module == removeModule)
                        {
                            Debug.LogError("Remove confirm newModule");
                            continue;
                        }
                        Assert.IsFalse(removeModule == module, "Module should not interrupt it self.");

                        Debug.LogError($"Confirm {module} Inturrpt {removeModule} {Current.Contains(removeModule)}");
                        RemoveCurrent(removeModule);
                        //removeModule.InterruptMotion(Limb);
                    }

                }

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                WaitConfirm.Clear();

            }
        }

        /// <summary>
        /// Queue motion.
        /// </summary>
        /// <param name="module"></param>
        void Enqueue(IMotionModule_Huamnoid module)
        {
            if (Current.Contains(module) || Queue.Contains(module))
                return;

            if (!module.CanQueue(out var queueNumber))
                return;

            if (DisableMotion)
                return;

            var length = Queue.Count;

            if (length > 0)
                for (int i = 0; i < length; i++)
                {
                    if (i >= queueNumber)
                        return;

                    if (Queue[i].CompareTo(module) > 0)
                    {
                        Queue.Insert(i, module);
                        return;
                    }
                }

            Queue.Add(module);

            Debug.Log($"Queue motion : {module}");
        }

        /// <summary>
        /// Remove motion from sequence when motion stop or cancel.
        /// </summary>
        public void RemoveMotion(IMotionModule_Huamnoid module)
        {
            if (WaitConfirm.WaitModule == module)
                WaitConfirm.WaitModule = null;

            if (Current.Remove(module))
            {
                Debug.LogError($"{Limb} remove motion {module}");
                Disqualify(module.HashCode);
                SubstituteMotion(null);
                return;
            }

            Queue.Remove(module);
        }

        // callback from SubstituteMotion
        public void SubstituteMotion(IMotionModule_Huamnoid ignoreModule = null)
        {
            if (DisableMotion)
                return;

            foreach (var queueModule in Queue)
            {
                if (queueModule.Equals(ignoreModule))
                    continue;

                if (CanMotion(queueModule,true) && queueModule.Ready2Motion(Limb))
                {
                    Debug.Log( Limb+" recovery newModule : " + queueModule.GetType());
                    Queue.Remove(queueModule);
                    ConfirmMotion(queueModule);
                    break;
                }
                else
                {
                    ConfirmMotion(null);
                }

            }
        }



        /// <summary>
        /// Disable a function ?todelete
        /// </summary>
        public void Dispose()
        {
            DisableMotion = true;

            foreach (var module in Current)
            {
                module.InterruptMotion(Limb);
                Disqualify(module.HashCode);
            }

            foreach (var module in Queue)
            {
                module.InterruptMotion(Limb);
            }

            Current.Clear();
            Queue.Clear();
        }

        void AddCurrent(IMotionModule_Huamnoid module)
        {
            var enable=Current.Add(module);
  
            Assert.IsTrue(enable);

            Debug.Log($"Verify sequence {module.GetData.name} : {enable}");

            Verify?.Invoke(module.HashCode, enable, Limb);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="module"></param>
        void RemoveCurrent(IMotionModule_Huamnoid module)
        {
            Debug.Log(Current.Contains(module));

            if (Current.Contains(module))
            {
                // remove current motion from list
                var valid = Current.Remove(module);
                Assert.IsTrue(valid, $"{Limb} failure to remove current sequence with {module}.");

                // disable current motion 
                Disqualify(module.HashCode);

                module.InterruptMotion(Limb);
            }

            // try enqueue current motion
            Enqueue(module);
        }

        /// <summary>
        /// Set motion sequence invalid. Conflict with interrupt??
        /// </summary>
        void Disqualify(int hash) 
        {
            Verify?.Invoke(hash, false, Limb);
        }

        public static Dictionary<int,LimbSequence> GetDictionary
        {
            get
            {
                var length = (int)Limb.LastLimb;
                var dic = new Dictionary<int, LimbSequence>(length);
                for (int i = 0; i < length; i++)
                {
                    var limb = (Limb)i;
                    dic.Add(i, new LimbSequence(limb));
                }

                return dic;
            }
        }
    }

}