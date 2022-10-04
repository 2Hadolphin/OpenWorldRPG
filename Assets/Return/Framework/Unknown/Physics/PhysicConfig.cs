using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using Zenject;


namespace Return
{
    /// <summary>
    /// Container to cache physic configs.  **LayerMask
    /// </summary>
    public class PhysicConfig : SingletonSO<PhysicConfig>//,IStart//,IInitializable
    {
        public static float Gravity { get; } = -9;



        [SerializeField]
        LayerMask m_WeaponPhysicMask;

        [SerializeField]
        LayerMask m_PlayerPhysicMask;

        [SerializeField]
        LayerMask m_QuickMask;

        [SerializeField]
        LayerMask m_SpawnPhysicMask;

        [SerializeField]
        LayerMask m_UIPhysicMask;

        [SerializeField]
        LayerMask m_VehiclePhysicMask;

        [SerializeField]
        LayerMask m_AnimationIKMask;

        [SerializeField]
        LayerMask m_ItemMask;

        [SerializeField]
        LayerMask m_ObstractLayer;

        [SerializeField]
        LayerMask m_CombatLayer;

        [Tooltip("55m/s On earth.")]
        [SerializeField]
        float m_terminalSpeed_Agent = 31f;

        public LayerMask WeaponPhysicMask { get => m_WeaponPhysicMask; protected set => m_WeaponPhysicMask = value; }
        public LayerMask PlayerPhysicMask { get => m_PlayerPhysicMask; protected set => m_PlayerPhysicMask = value; }
        public LayerMask QuickMask { get => m_QuickMask; protected set => m_QuickMask = value; }
        public LayerMask SpawnPhysicMask { get => m_SpawnPhysicMask; protected set => m_SpawnPhysicMask = value; }
        public LayerMask UIPhysicMask { get => m_UIPhysicMask; protected set => m_UIPhysicMask = value; }
        public LayerMask VehiclePhysicMask { get => m_VehiclePhysicMask; protected set => m_VehiclePhysicMask = value; }
        public LayerMask AnimationIKMask { get => m_AnimationIKMask; protected set => m_AnimationIKMask = value; }
        public LayerMask ItemMask { get => m_ItemMask; protected set => m_ItemMask = value; }
        public LayerMask ObstractLayer { get => m_ObstractLayer; protected set => m_ObstractLayer = value; }
        public LayerMask CombatLayer { get => m_CombatLayer; protected set => m_CombatLayer = value; }
        public float TerminalSpeed_Agent { get => m_terminalSpeed_Agent; set => m_terminalSpeed_Agent = value; }

  

        /// <summary>
        /// Ignore each other but detect other normals 
        /// </summary>
        public override void Initialize()
        {

        }

    }
}