using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class DynamicAssetsPool : Return.SceneModule.SceneModule
{
    [SerializeField]
    private int MaxItemNumbers = 300;


    //   Flag Preset
    [SerializeField]
    private Data_RespawnFlag ItemRespawnFlag;
    [SerializeField]
    private Data_RespawnFlag VehicleRespawnFlag;


    // Pool Slots
    [SerializeField]
    private DynamicPoolRoot[] items;
    private DynamicPoolRoot[] vehicles;

    private Dictionary<int, int> LookToDynamicPool;

    private SpawnsManager spawnsManager;

    private IPoolDebit[] LoanDebit;
    private Dictionary<int, Vector3Int> CheckDebit;    //  <LoanDebit.SN   ,  TimeHash-SN+OriginPoolSN >
    private Transform DAPStorage;
    private float BreakRate = 1;// 1==100% generate 0==0%
    private int DeclareNumber = 0;
    private float seed;
    private int InstanceSN = 0;


    private int Loan()
    {
        for (int i = 0; i < MaxItemNumbers; i++)
        {
            if (LoanDebit[i] == null)
            {
                return i;
            }
        }

        return -1;
    }

    public Vector3Int[] ApplyCredit(SpawnFlag.Types types, int PoolCode, int applyNums, out IPoolDebit[] debitCatches)
    {
        DynamicPoolRoot[] RootPools;
        int poolType;
        switch ((int)types)
        {
            case 0:
                RootPools = items;
                poolType = 0;
                break;
            case 1:
                RootPools = vehicles;
                poolType = 1;
                break;

            default:    //??
                RootPools = items;
                poolType = 0;
                break;

        }

        if (LookToDynamicPool.TryGetValue(PoolCode, out int sn))
        {
            int length = RootPools[sn].DebitCatch.Length;
            int debitSN;
            IPoolDebit debit;
            List<Vector3Int> debitV = new List<Vector3Int>(0);
            List<IPoolDebit> debitC = new List<IPoolDebit>(0);

            int i = 0;

            for (; i < length; i++)
            {
                if (applyNums > 0)
                {
                    debit = RootPools[sn].DebitCatch[i];
                    if (debit != null)
                    {
                        debitSN = Loan();
                        if (debitSN >= 0)    // <0 == too much thing in game deny the apply
                        {
                            int hash = debit.GetHashCode();

                            LoanDebit[debitSN] = debit;
                            RootPools[sn].DebitCatch[i] = null;

                            if (!CheckDebit.ContainsKey(debitSN))
                            {
                                CheckDebit.Add(debitSN, new Vector3Int(hash, poolType, sn));
                                debitV.Add(new Vector3Int(debitSN, hash, 0));
                                debitC.Add(debit);
                                applyNums--;
                            }
                        }
                        else
                        {
                            Debug.LogError("Too Much In Game, deny the apply !");
                        }
                    }
                }
            }

            debitCatches = debitC.ToArray();

            if (debitCatches.Length == 0)
                Debug.LogWarning("AssetPool is low level !");
            return debitV.ToArray();

        }
        else
        {
            Debug.LogError("Request CacheDebit didn't exist ! SpawnSN : " + PoolCode);
        }
        debitCatches = new IPoolDebit[0];
        return new Vector3Int[0];

    }



    public void DischargeCreadit(Vector3Int[] debit)
    {
        int length = debit.Length;
        int key;
        IPoolDebit debitCatch;
        for (int i = 0; i < length; i++)
        {
            key = debit[i].x;
            if (CheckDebit.ContainsKey(key))
                if (CheckDebit.TryGetValue(key, out Vector3Int confirmSN))
                    if (debit[i].y.Equals(confirmSN.x))
                    {
                        CheckDebit.Remove(key);
                        debitCatch = LoanDebit[key];
                        if (debitCatch != null)
                        {
                            debitCatch.Retrieve();
                            switch (confirmSN.y)
                            {
                                case 0:
                                    RetrieveDebit(debitCatch, items, confirmSN.z);
                                    break;
                                case 1:
                                    RetrieveDebit(debitCatch, vehicles, confirmSN.z);
                                    break;
                                default:
                                    RetrieveDebit(debitCatch, items, confirmSN.z);
                                    break;
                            }


                            debitCatch = null;
                        }

                    }
        }
    }

    private void RetrieveDebit(IPoolDebit debit, DynamicPoolRoot[] pool, int SN)
    {
        int length = pool[SN].DebitCatch.Length;
        for (int i = 0; i < length; i++)
        {
            if (pool[SN].DebitCatch[i] == null)
            {
                pool[SN].DebitCatch[i] = debit;
            }
        }
    }

    public override void Init()
    {
        spawnsManager = SpawnsManager.Manager;
        spawnsManager.Initialization();
        DAPStorage = new GameObject("DAPStorage").transform;
        DAPStorage.parent = this.transform;
        LookToDynamicPool = new Dictionary<int, int>();
        CheckDebit = new Dictionary<int, Vector3Int>();
        LoanDebit = new IPoolDebit[MaxItemNumbers];
        if (ItemRespawnFlag != null)
        {
            items = new DynamicPoolRoot[ItemRespawnFlag.PoolGenerateData.Length];
            LoadPool(items, ItemRespawnFlag);
        }

        if (VehicleRespawnFlag != null)
        {
            vehicles = new DynamicPoolRoot[VehicleRespawnFlag.PoolGenerateData.Length];
            LoadPool(vehicles, VehicleRespawnFlag);
        }
    }

    private void LoadPool(DynamicPoolRoot[] RootPool, Data_RespawnFlag data)
    {
        if (RootPool == null)
            return;
        if (RootPool.Length == 0)
            return;


        InstanceSN = 0;
        int poolVolume = 0;
        SpawnData spawn;
        int RootPoolLength = RootPool.Length;
        int Hash;
        for (int i = 0; i < RootPoolLength; i++)     // Add flag count to dynamicPool and regsit dictionary
        {
            RootPool[i] = new DynamicPoolRoot();
            Hash = (int)data.PoolGenerateData[i].x;
            RootPool[i].PoolHash = Hash;


            if (spawnsManager.LookHashToSpawn.TryGetValue(Hash, out spawn))  // Try Get Spawn from SpawnDictionary
            {
                RootPool[i].Spawn = spawn;
                LookToDynamicPool.Add(Hash, i); //Set Pool Dicitionary
            }
            else
            {
                Debug.LogError(" Spawn " + RootPool[i].PoolHash + " can't be found !");
            }
        }


        //-----------------------------------------------------------------------------------------
        DeclareNumber = 0;

        print("RootTransform Length = " + RootPoolLength);
        for (int i = 0; i < RootPoolLength; i++)
        {
            poolVolume = (int)data.PoolGenerateData[i].z;
            poolVolume=poolVolume>1000? poolVolume / 1000 : 1;
            print("Apply " + poolVolume + " rate of RootTransform Pool ");
            RootPool[i].ApplyNumber = poolVolume;
            DeclareNumber += poolVolume;
        }

        if (DeclareNumber > MaxItemNumbers)
        {
            BreakRate = (float)MaxItemNumbers / DeclareNumber;    // if under RefRate instance(true)
        }
        else
            BreakRate = 1;

        InstanceObjectToPool(RootPool);
    }



    private void InstanceObjectToPool(DynamicPoolRoot[] rootPools)
    {
        InstanceSN = 0;

        int RootPoolLength = rootPools.Length;
        int SpawnApplyNumbers;
        int SpawnRateTreeLength;

        float instanceSeed;
        int LootLength;
        GameObject ob;

  

        for (int rootPoolSN = 0; rootPoolSN < RootPoolLength; rootPoolSN++)    // Every RootTransform Pool
        {
            SpawnApplyNumbers = rootPools[rootPoolSN].ApplyNumber;
            print("Prepare to Generate " + SpawnApplyNumbers + " m_items from "+rootPools[rootPoolSN]);
            rootPools[rootPoolSN].DebitCatch = new IPoolDebit[SpawnApplyNumbers];

            for (int ApplyCount = 0; ApplyCount < SpawnApplyNumbers; ApplyCount++)      // Every Apply Number 
            {
                seed = Random.value;    //Take Seed
                if (seed < BreakRate)             // If seed under spawn rate(MaxNumber
                {
                    SpawnRateTreeLength = rootPools[rootPoolSN].Spawn.Assets.RateTree.Length; // Take Rate Tree Length
                    for (int SpawnRateTreeSN = 0; SpawnRateTreeSN < SpawnRateTreeLength; SpawnRateTreeSN++)       // Every Rate Tree 
                    {
                        //print(rootPools[rootPoolSN].Spawn.Assets.RateTree[SpawnRateTreeSN] + ":" + seed);
                        //print("Round " + SpawnRateTreeSN);
                        if (seed < rootPools[rootPoolSN].Spawn.Assets.RateTree[SpawnRateTreeSN])    // Get RateTree right Slot
                        {
                            if (SpawnRateTreeSN == 0) //items[0] == LootRateZone ()=> InstanceLoot 
                            {
                                //print("Generate Ob");
                                instanceSeed = Random.value;
                                LootLength = rootPools[rootPoolSN].Spawn.Assets.LootsContent.Loots.Length;
                                for (int lootSN = 0; lootSN < LootLength; lootSN++)
                                {
                                    //print(rootPools[rootPoolSN].Spawn.Assets.LootsContent.RateTree[lootSN]);
                                    if (rootPools[rootPoolSN].Spawn.Assets.LootsContent.RateTree[lootSN] > instanceSeed)
                                    {
                                        ob = rootPools[rootPoolSN].Spawn.Assets.LootsContent.Loots[lootSN].Element;
                                        //print(ob+" : "+instanceSeed);
                                        if (ob == null)
                                            continue;
                                        ob = Instantiate(ob,DAPStorage);
                                        print(rootPools.Length + "+" + rootPoolSN + "/" + rootPools[rootPoolSN].DebitCatch.Length + "+" + InstanceSN);
                                        if (!ob.TryGetComponent<IPoolDebit>(out rootPools[rootPoolSN].DebitCatch[ApplyCount]))        // CheckAdd Generated object
                                        {
                                            Debug.LogError(ob + " generate error ! Couldn't get IDebitCatch !");
                                            Destroy(ob);
                                            continue;
                                        }
                                        InstanceSN++;
                                        break;
                                    }
                                }

                            }
                            else            // >>0 (SpawnPools
                            {
                                //print("go next spawn");
                                if (rootPools[rootPoolSN].Spawn.Assets.Spawns.Length != 0)
                                    InstanceChildPool(rootPools[rootPoolSN].Spawn.Assets.Spawns[SpawnRateTreeSN - 1].Spawn, rootPoolSN, rootPools);
                            }

                        }
                    }
                }
            }
        }
        print(InstanceSN + " objects has been added to DAP ! ");
    }

    private void InstanceChildPool(SpawnData spawn, int rootPoolSN, DynamicPoolRoot[] rootPools)
    {
        int SpawnRateTreeLength;
        float instanceSeed;
        int LootLength;
        GameObject ob;
        int SpawnLength = spawn.Assets.Spawns.Length + 1;
        for (int SpawnLengthSN = 0; SpawnLengthSN < SpawnLength; SpawnLengthSN++)
        {
            //print("Try Generate " + SpawnLengthSN);
            seed = Random.value;    //Take Seed
            if (seed < BreakRate)             // If seed under spawn rate(MaxNumber
            {
                SpawnRateTreeLength = spawn.Assets.RateTree.Length - 1; // Take Rate Tree Length
                for (int SpawnRateTreeSN = 0; SpawnRateTreeSN < SpawnRateTreeLength; SpawnRateTreeSN++)       // Every Rate Tree 
                {
                    if (seed < spawn.Assets.RateTree[SpawnRateTreeSN])    // Get RateTree right Slot
                    {
                        instanceSeed = Random.value;
                        if (SpawnRateTreeSN == 0) //items[0] == LootRateZone ()=> InstanceLoot 
                        {
                            LootLength = spawn.Assets.LootsContent.Loots.Length;
                            for (int lootSN = 0; lootSN < LootLength; lootSN++)
                            {
                                if (spawn.Assets.LootsContent.RateTree[lootSN] < instanceSeed)
                                {
                                    ob = spawn.Assets.LootsContent.Loots[lootSN].Element;
                                    if (ob == null)
                                        continue;
                                    //print(ob);
                                    ob = Instantiate(ob,DAPStorage);
                                    if (!ob.TryGetComponent<IPoolDebit>(out rootPools[rootPoolSN].DebitCatch[InstanceSN]))        // CheckAdd Generated object
                                    {
                                        Debug.LogError(ob + " generate error ! Couldn't get IDebitCatch !");
                                        Destroy(ob);
                                        continue;
                                    }
                                    //print("Generate Succed ! " + rootPools[rootPoolSN].DebitCatch[InstanceSN]);
                                    InstanceSN++;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (spawn.Assets.Spawns.Length != 0)
                                InstanceChildPool(spawn.Assets.Spawns[SpawnRateTreeSN - 1].Spawn, rootPoolSN, rootPools);
                        }
                    }
                }
            }

        }

    }






}
