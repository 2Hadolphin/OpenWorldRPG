using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return
{
    /// <summary>
    /// Class to config execute order of scripts
    /// </summary>
    public class ExecuteOrderList
    {
        #region Framework
        /// <summary>
        /// ConstCache SystemCatch
        /// </summary>
        public const int Framework = -500;

        #endregion

        #region Container

        public const int Container = -250;

        #endregion


        #region Agent
        /// <summary>
        /// Agent data reference setup
        /// </summary>
        public const int AgentInitialize = -50;

        #endregion

        /// <summary>
        /// Motion system
        /// </summary>
        public const int ModuleSystem = -30;

        /// <summary>
        /// Motion module
        /// </summary>
        public const int ChildModules = -10;

        /// <summary>
        /// Audio player
        /// </summary>
        public const int IndependentChildModules = 10;

        public const int Camera = 500;


        public const int CameraEffect = 5000;


        public const int Early= -10;



        public const int Late = 10;

    }
}