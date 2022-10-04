using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Sirenix.OdinInspector;
using Return.Modular;
using System.Linq;
using TNet;

namespace Return.Agents
{

    public class HumanoidAgent : Agent
    {
        [Obsolete]
        [SerializeField]
        Category m_category;

        #region Iresolver

        public override void InstallResolver(IModularResolver resolver)
        {
            base.InstallResolver(resolver);
        }

        #endregion

        #region Setup

        public override void LoadTNO()
        {
            base.LoadTNO();

            Resolver.RegisterModule<ITNO>(tno);
        }

        protected override void LoadModules()
        {
            //InstanceIfNull(ref Input);
            //Resolver.RegisterModule<mPlayerInput>(Input);

            var modules = Resolver.Modules.ToHashSet();//.SelectWhere(x=>x is IModule,x=>x as IModule);

            Debug.Log(string.Join("\n", modules));

            foreach (var module in modules)
            {
                if (module.Equals(this))
                    continue;

                //module.SetHandler(this);

                if (module is IModule handle)
                {
                    if (handle.CycleOption.HasFlag(ControlMode.Register))
                        handle.Register();
                }
            }


            foreach (var module in modules)
            {
                if (module.Equals(this))
                    continue;

                if (module is IModule handle)
                {
                    if (handle.CycleOption.HasFlag(ControlMode.Activate))
                        handle.Activate();
                }
            }

            //var modules = GetComponents<IModularHandle>();

            //foreach (var module in modules)
            //    RegisterSystem(module);
        }

        protected override void LoadIdentity()
        {

        }

        #endregion

        #region Routine

        //[ShowInInspector, ReadOnly]
        //protected InputWrapper Input;


        protected override void Activate()
        {
            //Input.enabled = true;
            base.Activate();
        }

        protected override void Deactivate()
        {
            //Input.enabled = false;
            base.Deactivate();
        }

        #endregion


        #region Obsolete

        //[ShowInInspector, ReadOnly]
        //[Obsolete]
        //protected HashSet<ClassWrapper> Systems = new ();

        /// <summary>
        /// Find enable system on this character to register **MotionSystem **InventorySystem **StateSystem
        /// </summary>
        //public override bool Authorize<T>(out T system, bool fuzzySearch = true)
        //{
        //    system = default;

        //    if (typeof(T).IsSubclassOf(typeof(UnityEngine.Object))
        //        && Systems.TryGetValue(new ClassWrapperInjector(typeof(T)), out var wrapper)
        //        && wrapper
        //        && wrapper.System is T target)
        //        system = target;
        //    else if (fuzzySearch)
        //    {
        //        var enumerator = Systems.GetEnumerator();
        //        while (enumerator.MoveNext())
        //        {
        //            if (enumerator.Current.System is T mSystem)
        //            {
        //                system = mSystem;
        //                break;
        //            }
        //        }
        //    }

        //    if(system==null)
        //        Debug.Log("Missing system when agent module authorize : "+typeof(T));


        //    return system is not null;
        //}

        #endregion

    }
}