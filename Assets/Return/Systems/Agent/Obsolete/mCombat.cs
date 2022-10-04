//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Return;
//using System.Linq;
//using Sirenix.OdinInspector;
//using Return.m_items;

//public enum StategyMode { Hide, Escape, GetShelter, Attack, Spy }

//public class mCombat : MonoBehaviour, ICombat
//{
//    public HiveAgent HiveAgent;
//    public ReadOnlyTransform tf { get; set; }

//    public Animator anim;
//    public TeamBase @base;

//    public byte Team { get; set; }


//    public float EnableRange 
//    {
//        set=>PowSensorRange  = Mathf.Pow(value, 2);
//    }
//    public float PowSensorRange;

//    public event System.Action<float> HPPost;

//    int hp;
//    public bool HasTarget => nearest != null;

//    public int HealthPoint 
//    {
//        get => hp;
//        set
//        {
           
//            hp = value;
//            HPPost?.Invoke(value);

//            if (value <= 0)
//            {
//                AgentBlackboard.m_Instance.Kill(Team, tf, out var @base);
//                @base?.Reborn(HiveAgent);
//            }
 
//        }
//    }
//    public int ActiveMembers => Teammate.Count;


//    public List<ICombat> Teammate=new List<ICombat>();
//    [ShowInInspector]
//    public List<ICombat> enemies = new List<ICombat>();


//    public void ReciveDamage(int dmg)
//    {
//        HealthPoint -= dmg;

//        if (mRandom.RandomTrue(0.3f))
//            mAudioManager.PlayOnceAt(HiveAgent.CharacterInfo.HurtEffect.ToArray.Random().Clips.Random(), tf.position);

//    }
//    public void ReciveDamage()
//    {
//        //HealthPoint -= (int)data.Damage;

//        if (mRandom.RandomTrue(0.3f))
//            mAudioManager.PlayOnceAt(HiveAgent.CharacterInfo.HurtEffect.ToArray.Random().Clips.Random(), tf.position);
//    }

//    public void Move(Vector3 pos)
//    {
//        HiveAgent.Move(pos);
//    }

//    ReadOnlyTransform upChest;
//    ReadOnlyTransform item;
//    ReadOnlyTransform PrimeHandle;
//    ReadOnlyTransform SecondHandle;

//    public PR ItemOffset;
//    public PR ItemMov;
//    public Vector3 RightHint;
//    public Vector3 LeftHint;
//    public ReadOnlyTransform target;
//    private void Awake()
//    {
//        tf = transform;
//        CharacterRoot.InstanceIfNull(ref anim);

//        upChest = anim.GetBoneTransform(HumanBodyBones.UpperChest);


//        HiveAgent = GetComponent<HiveAgent>();

//        var wp = DemoPreset.Instance.Weapons.Random();
//        var pose = wp.HandPose;

//        item = Instantiate(wp.Model, upChest).transform;
//        if(pose.GetOffset(Return.Humanoid.Limb.RightHand,out ItemOffset))
//        item.SetLocalPR(ItemOffset);


//        PrimeHandle = item.Find("Handle_Prime");
//        SecondHandle = item.Find("Handle_Second");

//        ItemMov = ItemOffset;

//        target = new GameObject("AgentTarget").transform;

//        //wp.Info.Modules?.LoadModule(item.CharacterRoot);
//        //Weapon = item.GetComponent<FirearmsModulePreset.mmgun>();
//        //Weapon.target = new Coordinate_Offset(target,Vector3.up,Quaternion.identity,false);
//        //Weapon.Team = Team;
//        EnableRange = 15;

//        HiveAgent.IK.Delegate(Side.Right, out RightHand,out RightElbow);
//        HiveAgent.IK.Delegate(Side.Left, out LeftHand,out LeftElbow);

//        Config(RightHand);
//        Config(RightElbow);
//        Config(LeftHand);
//        Config(LeftElbow);
//    }
//    public Coordinate_Delegate RightHand;
//    public Coordinate_Delegate RightElbow;
//    public Coordinate_Delegate LeftHand;
//    public Coordinate_Delegate LeftElbow;

//    void Config(Coordinate_Delegate @delegate)
//    {
//        @delegate.OutputPositionSpace = Space.World;
//        @delegate.OutputRotationSpace = Space.World;
//        @delegate.Weight = 1;
//        @delegate.PositionWeight = 1;
//        @delegate.RotationWeight = 1;


//    }

//    private void OnEnable()
//    {
     

//        StartCoroutine(TeammateSensor());
//        AgentBlackboard.m_Instance.RegisterHandler(this);

//    }

//    private void OnDisable()
//    {
//        AgentBlackboard.m_Instance.Unregister(this);
//    }

//    private void Update()
//    {
//        SetWeaponTarget();

//        if (PrimeHandle)
//        {
//            RightHand.SetPosition = PrimeHandle.position;
//            RightHand.SetRotation = PrimeHandle.rotation.eulerAngles;

//            var hintPos = Vector3.Lerp(RightHand.position, RightHand.OriginPosition, 0.5f);

//            Vector3 up;
//            if (RightHand.OutputPositionSpace == Space.World)
//                up = RightHand.ReadOnlyTransform.up;
//            else
//                up = Vector3.up;

//            hintPos -= up;
//            RightElbow.SetPosition = hintPos;

//        }

//        if (SecondHandle)
//        {
//            LeftHand.SetPosition = SecondHandle.position;
//            LeftHand.SetRotation = SecondHandle.rotation.eulerAngles;

//            var hintPos = Vector3.Lerp(LeftHand.position, LeftHand.OriginPosition, 0.5f);

//            Vector3 up;
//            if (LeftHand.OutputPositionSpace == Space.World)
//                up = LeftHand.ReadOnlyTransform.up;
//            else
//                up = Vector3.up;

//            hintPos -= up;
//            LeftElbow.SetPosition = hintPos;
//        }

//    }
//    bool init;

//    void SetWeaponTarget()
//    {
//        Vector3 pos;
//        if (nearest == null)
//            pos = tf.position + tf.forward.Multiply(100f);
//        else
//            pos = nearest.tf.position;


//        target.position = Vector3.Lerp(target.position,pos,ConstCache.deltaTime*20);
//    }
//    [ShowInInspector]
//    ICombat nearest;
//    private void FixedTick()
//    {
//        if (enemies.Count == 0)
//        {
//            return;
//        }



//        var min = float.MaxValue;
//        nearest = null;
//        foreach (var enemy in enemies)
//        {
//            if (enemy.tf == null)
//                continue;

//            var d = (tf.position - enemy.tf.position).sqrMagnitude;
//            if (d < min)
//            {
//                nearest = enemy;
//                min = d;
//            }
//        }

//        if (nearest != null)
//        {
//            if (min < Mathf.Pow(/*Weapon.data.Distance*/0, 2))
//            {
//                var t = nearest.tf;
//                if (Vector3.Dot(t.position - tf.position, tf.forward) < 0.7f)
//                {

//                    target.position = nearest.tf.position;

//                    var c = Random.value;

//                    //if (c > 0.75f)
//                    //    Weapon.Fire(default);
//                    //else if (c > 0.35f)
//                    //    HiveAgent.Move(nearest.tf.position);
//                    //else
//                    //    Weapon.Release(default);
//                    //return;
//                }

//                GetHide();
//                return;
//            }

//            HiveAgent.Move(nearest.tf.position);
//        }
//    }

//    public void GetHide()
//    {
//        var shelter=GetStategyValue();
//        if (shelter&&nearest.tf)
//            getShelter(nearest.tf, shelter);
//    }

//    IEnumerator TeammateSensor()
//    {
//        yield return ConstCache.WaitForSeconds(3f);
//        var wait = ConstCache.WaitForOneSecond;
//        Teammate.Clear();
//        enemies.Clear();

//        while (enabled)
//        {
//            var pos = transform.position;
//            var nearby=AgentBlackboard.m_Instance.GetAgents.FindClose(pos);

//            foreach (var agent in nearby)
//            {
//                if ((agent.transform.position - pos).sqrMagnitude > PowSensorRange)
//                    continue;


//                if (agent.Team != Team)
//                {
//                    if (!enemies.Contains(agent))
//                        enemies.Add(agent);

//                    HiveAgent.agent.ResetPath();
//                }
//                else
//                {
//                    if (!Teammate.Contains(agent))
//                        Teammate.Add(agent);
//                }
//            }
//            yield return wait;
//        }
//    }
//    [SerializeField]
//    private float StategySensorRange=10;



//    public ColliderBounds GetStategyValue()
//    {
//        ColliderBounds[] colliders= Physics.OverlapSphere(this.transform.position, StategySensorRange, mPhysicSetting.Instance.CombatLayer);
//        var values=new List<StrategyValue>();
//        ColliderBounds col=colliders.Random();

//        return col;
//    }

//    public void getShelter(ReadOnlyTransform tf,ColliderBounds colliderr)
//    {

//        Vector3 direction = (colliderr.bounds.center - tf.position);

//        if(colliderr is MeshCollider)
//        {
//            HiveAgent.Move(tf.position + direction);
//        }
//        else if (colliderr.isTrigger)
//        {
//            HiveAgent.Move(colliderr.ClosestPoint(colliderr.bounds.center + Random.insideUnitSphere * colliderr.bounds.min.magnitude));
//        }
//        else
//        {
//            direction.Normalize();
//            HiveAgent.Move(colliderr.ClosestPoint(colliderr.bounds.center + (direction * 2 + Random.insideUnitSphere) * colliderr.bounds.size.magnitude));
//        }
//    }



//    private void OnDrawGizmos()
//    {
//        Gizmos.color = Color.red;
//        Gizmos.DrawWireSphere(transform.position, PowSensorRange.d_Sqrt());
//    }


//}



//public interface ICombat
//{
//    ReadOnlyTransform tf { get; }
//    //void ReciveDamage(FirearmsModulePreset.WeaponData data);
//    byte Team { get; }

//    void Move(Vector3 pos);
//}