using UnityEngine;
using System.Collections;
using Return.Items;
using System.Collections.Generic;
using System.Linq;

namespace Return.InteractSystem
{
    /// <summary>
    /// ???????
    /// </summary>
    public class DetectSelectZone : BaseComponent
    {
        // Usually Ref
        private Transform _TF;
        //private SelectionUGUI _SAS;

        [SerializeField]
        [Range(1, 10)]
        private float sensorRange = 1;
        [SerializeField]
        [Range(1, 10)]
        private float sensorRate = 0.5f;
        [SerializeField]
        [Range(1, 100)]
        private int DetectableNums = 20;

        [SerializeField]
        private LayerMask mask;
        private Collider[] NewTargets;
        private Collider[] lastTargets;
        private List<Collider> CurringTargets;
        private Dictionary<Collider, IItem> ExistObjs;

        private int targetNums;
        private bool active;
        private WaitForSeconds wait;


        //public  void Init(HumanoidAgent_Exhibit reference)
        //{

        //    active = false;

        //    ExistObjs = new Dictionary<ColliderBounds, m_items.IItem>();
        //    NewTargets = new ColliderBounds[DetectableNums];



        //    CurringTargets = new List<ColliderBounds>();
        //    wait = new WaitForSeconds(sensorRate);
        //    //_TF = Agent.root;
        //    //_SAS = Agent.I.SelectAS;

        //}

        private void OnEnable()
        {
            StartCoroutine(SensorZone());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }



        IEnumerator SensorZone()
        {
            ExistObjs = new Dictionary<Collider, IItem>(DetectableNums);
            NewTargets = new Collider[DetectableNums];
            CurringTargets = new List<Collider>();
            lastTargets = new Collider[0];
            for (; ; )
            {
                yield return wait;
                if (CensorNearbyObjects())
                {
                    CurringTargets.Clear();
                    Collider collider;
                    IItem item;
                    for (int i = 0; i < targetNums; i++)        //  add new
                    {
                        collider = NewTargets[i];
                        CurringTargets.Add(collider);
                        if (!ExistObjs.ContainsKey(NewTargets[i]))
                        {
                            item = collider.GetComponent<IItem>();
                            ExistObjs.Add(collider, item);
                            //_SAS.AddObj(item);
                        }
                    }

                    targetNums = lastTargets.Length; //Remove losted target in selectAssistor
                    for (int i = 0; i < targetNums; i++)
                    {
                        collider = lastTargets[i];
                        //if (!CurringTargets.Contains(collider))
                        //    _SAS.RemoveObj(ExistObjs[collider]);
                    }

                    lastTargets = NewTargets;
                }
                else
                {
                    //_SAS.RemoveAllObjs();
                    lastTargets = new Collider[0];
                }
            }
        }

        private bool CensorNearbyObjects()
        {
            targetNums = Physics.OverlapSphereNonAlloc(_TF.position, sensorRange, NewTargets, mask);

            if (targetNums > 0)
            {
                if (!active)
                {
                    active = true;
                    //Agent.I.SelectSS.Acitvate = true;
                }
            }
            else
            {
                if (active)
                {
                    active = false;
                    //Agent.I.SelectSS.Acitvate = false;
                }
            }

            return active;
        }

        private void OnDrawGizmosSelected()
        {
            if (_TF != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_TF.position, sensorRange);
            }
        }


    }
}