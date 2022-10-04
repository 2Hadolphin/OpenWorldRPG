using UnityEngine;
using Return.Agents;
using UnityEngine.Assertions;
using TNet;
using Sirenix.OdinInspector;
using System;

namespace Return.Items.Weapons.Firearms
{

    /// <summary>
    /// Load host or remote module
    /// </summary>
    public abstract class FirearmsModule : ConfigurableItemModule
    {

    }

    public abstract class FirearmsModule<T> : FirearmsModule, IFirearmsModule where T : FirearmsModulePreset
    {
        protected override ConfigurableItemModulePreset Preset
        {
            get => pref;
            set => value.Fill(ref pref);
        }

        [ShowInInspector, ReadOnly]
        protected T pref;

        /// <summary>
        /// inject ITNO
        /// </summary>
        [Obsolete]
        public virtual Firearms Firearms { get; protected set; }

        public override void SetHandler(IItem module)
        {
            base.SetHandler(module);

            Firearms = module as Firearms;
            Assert.IsNotNull(Firearms);
        }
    }
}