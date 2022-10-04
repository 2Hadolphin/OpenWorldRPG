using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.Inventory
{
    /// <summary>
    /// Cache inventory content.
    /// </summary>
    public class ArchiveContent : NullCheck,IArchiveContent
    {
        public object content { get; set; }

        public uint Volume { get; set; } = 1; 

        public bool Equals(IArchiveContent other)
        {
            return content == other.content;
        }


    }
}