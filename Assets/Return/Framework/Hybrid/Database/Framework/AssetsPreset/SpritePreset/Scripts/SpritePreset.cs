using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Return.Database.Assets
{
    public class SpritePreset : AssetsPreset<Sprite>
    {
        [SerializeField][ListDrawerSettings(Expanded =true)]
        public List<string> Tags=new ();


    }
}