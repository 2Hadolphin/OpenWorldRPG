using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TNet
{
    public class ChannelList
    {
        public const int Framework = 7;
        public const int Agent = 13;



        /// <summary>
        /// Channnels for framework.
        /// </summary>
        public static readonly int[] Channels = new[]
        {
            Framework,
            Agent,
        };
    }
}