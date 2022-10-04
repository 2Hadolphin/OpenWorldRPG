//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using Sirenix.OdinInspector;
//using Return;

//using System.Linq;
//using System.Collections;
//using Return.Agent;
//using Return.Demo;

//public class AgentBlackboard : SingletonMono<AgentBlackboard>
//{
//    protected KdTree<mCombat> Routers = new KdTree<mCombat>();

//    [ShowInInspector]
//    protected HashSet<mCombat> Lock = new HashSet<mCombat>();
//    public KdTree<mCombat> GetAgents { get => Routers; }

//    public int TeamNums=2;
//    public int TeamPlayer=5;

//    public Dictionary<byte, TeamBase> BaseCatch=new Dictionary<byte, TeamBase>();
//    public List<TeamBase>RebornBases=new List<TeamBase>();

//    public ReadOnlyTransform[] BasePoints;

//    public TeamBase BornPointPrefab;

//    public Return.Agent.CharacterInfo[] AgentPrefabs;



//    public void RegisterHandler(mCombat router)
//    {
//        if (Lock.Add(router))
//            Routers.Add(router);
//    }

//    public void Unregister(mCombat router)
//    {
//        if (Lock.Remove(router))
//            Routers.RemoveAll((width) => width == router);
//    }

//    protected override void Awake()
//    {
//        base.Awake();
 

//    }

//    public Vector3 GetMission(byte team)
//    {
//        var pos = RebornBases.Where(width => width.TeamCode != team).ToArray().Random();
//        return pos?pos.transform.position:Vector3.zero;
//    }

//    private void Start()
//    {
//        //var doll = DemoPreset.Instance.Ragdoll;
//        //Ragdolls = new mRagdoll[TeamNums];
//        //for (int i = 0; i < TeamNums; i++)
//        //{
//        //    Ragdolls[i] = Instantiate(doll, Vector3.zero,Quaternion.identity);
//        //}

//        AgentPrefabs = CharacterArchives.Instance.ToArray;
//        BuildTeam();
//        GameScore.m_Instance.CharacterRoot.SetActive(true);
//    }

//    public void BuildTeam()
//    {
//        var list = new List<ReadOnlyTransform>(BasePoints);

//        for (int i = 0; i < TeamNums; i++)
//        {
//            var p = list.Random();
//            list.Remove(p);
//            var @base = Instantiate(BornPointPrefab, p, false);
//            RebornBases.Add(@base);
//            var team = (byte)i;
//            @base.TeamPlayer = TeamPlayer;
//            @base.TeamCode = team;
//            BaseCatch.Add(team, @base);
//            AddTeam(team);
//        }
//    }

//    //public void AssignPlayer(PlayerCombat combat)
//    //{
//    //    var @base = RebornBases.Random();
//    //    @base.Clearance();
//    //    combat.Team = @base.TeamCode;
//    //    combat.MaxHP = @base.HealthPoints;
//    //    combat.HP = @base.HealthPoints;
//    //    combat.tf.SetWorldPR(@base.GetRebornPoint());
//    //    combat.MarkTeammate();
//    //}

//    public void ResetTeam(int champ,int teammate)
//    {
//        Routers.Clear();
//        Lock.Clear();
//        BaseCatch = new Dictionary<byte, TeamBase>(champ);
//        var length = RebornBases.Count;
//        for (int i = 0; i < length; i++)
//        {
//            Destroy(RebornBases[i].CharacterRoot);
//        }
//        TeamNums = champ;
//        TeamPlayer = teammate;
//        RebornBases = new List<TeamBase>(TeamNums);


//        BuildTeam();
//    }

//    private void OnEnable()
//    {
//        StartCoroutine(UpdateData());
//    }

//    IEnumerator UpdateData()
//    {
//        var wait = ConstCache.WaitForSeconds(3f);

//        while (enabled)
//        {
//            Routers.UpdatePositions();
//            yield return wait;
//        }
//    }



//    public void PostCombatNoise(Vector3 position,byte team)
//    {
//        var run=Routers.GetEnumerator();

//        while (run.MoveNext())
//        {
//            var agent=run.Current;
//            if (agent.Team == team)
//                continue;
//            else
//                agent.Move(position);
//        }


//    }

//    public mRagdoll[] Ragdolls;
//    public int sn;

//    mRagdoll GetRagdoll
//    {
//        get
//        {
//            sn = Ragdolls.Loop(sn + 1);
//            return Ragdolls[sn];
//        }
//    }


//    public void Kill(byte teamCode,ReadOnlyTransform tf,out TeamBase @base)
//    {

//        if(BaseCatch.TryGetValue(teamCode,out @base))
//        {
//            GameScore.m_Instance.AddScore(teamCode, -1);

//            var ragdoll = GetRagdoll;
//            ragdoll.Substitute(tf);
//        }

//    }


//    public void KillTeam(byte team)
//    {
//        var length = RebornBases.Count;
        
//        for (int i = 0; i < length; i++)
//        {
//            var @base = RebornBases[i];
//            if (@base.TeamCode == team)
//            {
//                RebornBases.Remove(@base);
//                if (RebornBases.Count ==1)
//                {

//                    ResetGame();
//                    return;
//                }


//                Destroy(@base.CharacterRoot);

//                break;
//            }
//        }
 
//    }



//    public void ResetGame()
//    {
//        DemoGameSetting.m_Instance.CharacterRoot.SetActive(true);
//        if (DemoManager.m_Instance.PlayerCombat.Team == RebornBases[0].TeamCode)
//            DemoGameSetting.m_Instance.Win();
//        else
//            DemoGameSetting.m_Instance.Lose();
//    }

//    public void AddTeam(byte team)
//    {
//        GameScore.m_Instance.AddTeam(team);

//    }


//}
