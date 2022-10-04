using UnityEngine;
using System;

namespace Return.Definition
{
    [Serializable]
    public struct CommonData
    {
        public string ID;
        public string UID;
    }
    [Serializable]
    public struct CreatureData
    {
        public int Health;
        public int Energy;
        public int Hungry;
        public int Thirst;
        public int Mind;
        public int Level;
    }

    [Serializable]
    public struct AgentData
    {
        public int Strength;
        public int Endurance;
        public int BastBandwidth;
        public int Intelligence;
        public int LearningCoefficient;
        public int Program;
        public int Agility;
        public int Dexterity;
        public int Insight;
        public int Faith;
        public int Individual;
        public int Pack;
        public int Luck;
    }
    [Serializable]
    public struct RigData
    {
        LocateRef LocateRef;
        public rigState State;
        public Vector3 StorgePos;
        public int[] StorgeLink; // RefObject.HashCode.value
    }

    public enum IType { Item, Object, Vehicle, Player, Agent }
    public enum LocateRef { world = 0, vehicle = 1, building = 2 }
    public enum rigState { Root_Static = 0, Root_Dynamic = 1, Child_Static = 2, Child_Dynamic = 3 }

    public class CommonState
    {









    }
}