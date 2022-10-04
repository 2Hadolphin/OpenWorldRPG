using UnityEngine;
using Sirenix.OdinInspector;
using Return;

namespace Return.Database
{

    /// <summary>
    /// Basic data class of return game object preset. (**Item **Character **Vehicle **Building)  
    /// </summary>
    public abstract class BasicInfo : PresetDatabase
    {
        //#region UnityEditor
        //protected const string LEFT_VERTICAL_GROUP = "Split/Left";
        //protected const string STATS_BOX_GROUP = "Split/Left/Stats";
        //protected const string GENERAL_SETTINGS_VERTICAL_GROUP = "Content";
        //#endregion

        //[BoxGroup(LEFT_VERTICAL_GROUP + "/General Settings")]
        public int GUID;
        //[BoxGroup(LEFT_VERTICAL_GROUP + "/General Settings")]
        public string Name;
        //[BoxGroup(LEFT_VERTICAL_GROUP + "/General Settings")]
        public string ID;


        //[BoxGroup(STATS_BOX_GROUP)]
        public RarityLevel Rarity;

    }


}
