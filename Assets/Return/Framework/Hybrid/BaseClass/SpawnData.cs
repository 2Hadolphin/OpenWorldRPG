using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;

[CreateAssetMenu(fileName = "RootTypes", menuName = "MyData/Spawns/Data", order = 1)]
public class SpawnData : ScriptableObject
{
    public int HashCode;
    public int GUID;
    public string CollectionName;
    public List<SpawnData> Parents;
    public SpawnMain Assets;


    [Serializable]
    public struct SpawnMain
    {
        public float[] RateTree;
        public SpawnContent[] Spawns;
        public LootContent LootsContent;
        public int LootsWeight;
        public float LootsRate;
    }

    [Serializable]
    public struct SpawnContent
    {
        public SpawnData Spawn;
        public int Weight;
        public float Rate;
    }
    [Serializable]
    public struct LootContent
    {
        public Loot[] Loots;
        public float[] RateTree;
    }
    [Serializable]
    public struct Loot 
    {
        public GameObject Element;
        public int Weight;
        public float Rate;
    }

}
