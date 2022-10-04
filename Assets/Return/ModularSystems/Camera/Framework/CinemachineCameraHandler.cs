using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Sirenix.OdinInspector;

namespace Return.Cameras
{
    public class CinemachineCameraHandler : BaseComponent
    {
        [SerializeField,Required]
        CinemachineBrain m_Brain;

        public CinemachineBrain CinemachineBrain => m_Brain;



        public CinemachineVirtualCameraBase VirtualCamera { get; set; }
    }
}