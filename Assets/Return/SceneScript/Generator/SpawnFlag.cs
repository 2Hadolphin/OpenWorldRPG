using UnityEngine;
using System.Collections;
using Return.CentreModule;

public class SpawnFlag : MonoBehaviour,ISpawnFlag
{
    public enum Types { Item=0, Vehicle=1, Animal=2, Resource=3, Object=4, Player }
    public enum PlaceType { MathfCast, Rigbody, None }
    [SerializeField]
    private Types Type;
    [SerializeField]
    private PlaceType placeType;
    [SerializeField]
    private BoxCollider zone=null;

    public Types GetSpawnType { get { return Type; } }
    [SerializeField]
    private Vector3Int FlagSetting;

    [SerializeField]
    private string[] PoolCollection;
    [SerializeField]
    private int[] PoolsHashCode;

    [SerializeField][Range(0,1)]
    private float respawnRate=1;
    [SerializeField][Range(0,10)]
    private int RespawnNumbers=1;

    private Vector3Int[] Debit;

    private Coroutine coroutine;
    private bool activate = false;
    public bool Ban { get; private set; }

    public void Initialization(Data_RespawnFlag.Flag  data)
    {
        gameObject.transform.position = data.position;
        gameObject.transform.rotation = Quaternion.Euler(data.rotation);
        zone.center = data.BoundPos;
        zone.size = data.BoundSize;

        placeType = data.PlaceType;
        respawnRate = data.RespawnRate;
        RespawnNumbers = data.RespawnNumber;
        if (data.PoolCollection.Length > 0)
        {
            PoolCollection = new string[data.PoolCollection.Length];
            PoolCollection = data.PoolCollection;
            PoolsHashCode = data.PoolCollectionHash;
            
        }



    }

    public Data_RespawnFlag.Flag storeFlag()
    {
        return new Data_RespawnFlag.Flag
        {
            position = transform.position,
            rotation = transform.rotation.eulerAngles,
            PlaceType = placeType,
            RespawnRate = this.respawnRate,
            BoundPos = zone.center,
            BoundSize = zone.size,
            PoolCollection = this.PoolCollection,
            PoolCollectionHash = this.PoolsHashCode,
            RespawnNumber = this.RespawnNumbers
        };
    }

    public void RefHashCode()
    {
        var Manager = SpawnsManager.Manager;

        if (PoolCollection == null)
            PoolCollection = new string[0];

        int length = PoolCollection.Length;

        PoolsHashCode = new int[PoolCollection.Length];

        for (int i = 0; i < length; i++)
        {
            if (!Manager.LookNameToHash.TryGetValue(PoolCollection[i], out PoolsHashCode[i]))
                Debug.LogError("Flag : " + this.gameObject + " contain a invalid collection name !");
        }

    }

    public void Activate()
    {
        if (activate)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }
        else
        {
            activate = true;
            AssignObjects();
        }
    }

    public void CheckVisa()
    {
        coroutine = StartCoroutine(WaitNewVisa());
    }

    IEnumerator WaitNewVisa()
    {
        yield return new WaitForSeconds(7);

        ReturnObjects();
        activate = false;
        yield break;
    }

    private void AssignObjects()
    {
        int sn = Random.Range(0, PoolsHashCode.Length);
        print("********ApplyObjectRespawn "+PoolsHashCode.Length + " (length/sn) " + sn);

        Debit = GDR.data.Module.assetsGenerator.
            Ref.DAP.ApplyCredit(Type,PoolsHashCode[sn], RespawnNumbers, out IPoolDebit[] debits);

        int length = debits.Length;
        for(int i = 0; i < length; i++)
        {
            if (placeType == PlaceType.MathfCast)
            {
                debits[i].Spawn(zone.bounds,false);
            }
            else if (placeType == PlaceType.Rigbody)
            {
                debits[i].Spawn(zone.bounds, true);
            }
            else
            {
                debits[i].Spawn(transform.position,transform.rotation);
            }

        }
    }

    private void ReturnObjects()
    {
        GDR.data.Module.assetsGenerator.Ref.DAP.DischargeCreadit(Debit);
    }



    private void OnDrawGizmos()
    {
        if (zone!=null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position+transform.TransformDirection(zone.center), zone.size);

            if (coroutine != null)
                Gizmos.color = Color.yellow;
            if (!activate)
                Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + transform.TransformDirection(zone.center) + Vector3.up * 3, 1);

        }
    }


}

