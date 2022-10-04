using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Return.CentreModule;

namespace Return.SceneModule
{
    public class ActiveZoneManager : SceneModule
    {

        [SerializeField]
        private float SearchRange = 50;
        private float SearchRangeSqr = 0;
        [SerializeField]
        private LayerMask mask=default;
        private Transform origin;

        private Collider[] Zones;
        private int Nums;
        private ISpawnFlag[] Flags;

        [SerializeField]
        private float UpdateTime = 5;


        private Coroutine coroutine;
        private WaitUntil waitB;
        private WaitForSeconds waitT;
        private Vector3 lastPos = Vector3.zero;
        public int Volume { get; private set; } //Limit spawn object size and numbers



        public override void Init()
        {
            Zones = new Collider[0];
            Flags = new ISpawnFlag[0];
            Volume = 10; //??
            mask = GDR.RespawnPhysicMask;
            SearchRangeSqr = SearchRange * SearchRange * 4 / 9;
        }


        public void LoadBeacon(Transform tf)
        {
            origin = tf;
            StartCoroutine(UpdateManager());
        }

        public void RemoveBeacon()
        {
            StopAllCoroutines();
            origin = null;
            StartCoroutine(DeactivateFlags());

        }

        IEnumerator UpdateManager()
        {
            lastPos = origin.position;
            waitT = new WaitForSeconds(UpdateTime);
            LoadFlags();
            for (; ; )
            {
                yield return waitT;
                if ((origin.position - lastPos).sqrMagnitude > SearchRangeSqr)
                {
                    lastPos = origin.position;
                    LoadFlags();
                }
            }
        }


        //........................................................................Local

        public void LoadFlags()
        {
            if (origin != null)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);

                coroutine = StartCoroutine(DeactivateFlags());
            }

        }

        IEnumerator DeactivateFlags()
        {
            int nums = Flags.Length;
            for (int i = 0; i < Nums; i++)
            {
                if (Flags[i] != null)
                    Flags[i].CheckVisa();
                yield return null;
            }
            coroutine = null;
            SearchZones();
            yield break;
        }


        private void SearchZones()
        {
            if (origin != null)
            {
                Zones = UnityEngine.Physics.OverlapSphere(origin.position, SearchRange, mask, QueryTriggerInteraction.Collide);

                Nums = Zones.Length;
                List<ISpawnFlag> spawnFlags = new List<ISpawnFlag>(Nums);
                ISpawnFlag Newflag;

                for (int i = 0; i < Nums; i++)
                {
                    if (Zones[i].TryGetComponent<ISpawnFlag>(out Newflag))
                    {
                        spawnFlags.Add(Newflag);
                    }
                    else
                    {
                        Debug.LogError("Spawn Point Interface didn't exist ! - " + Zones[i].gameObject);
                    }
                }
                Flags = spawnFlags.ToArray();
                print("Succed Loaded "+Flags.Length + " Flag ");

                if(coroutine==null)
                coroutine = StartCoroutine(ActivateFlags());
            }
        }

        IEnumerator ActivateFlags()
        {
            ISpawnFlag flag;
            int nums = Flags.Length;
            print("Active Flag numbers : " +nums);
            for (int i = 0; i < nums; i++)
            {
                flag = Flags[i];
                if (!flag.Ban)
                    Flags[i].Activate();
                yield return null;
            }

            coroutine = null;
            yield break;
        }



        private void OnDrawGizmos()
        {
            if (origin != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(lastPos, SearchRange);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(origin.position, SearchRange * 2 / 3);
            }
        }


    }



}