//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class BattleCentral : MonoBehaviour
//{
//    [SerializeField]
//    private AgentScripts scripts=null;
//    [SerializeField]
//    private mCombat combatCode=null;

//    [SerializeField]
//    private LayerMask VisionMask;

//    public StrategyValue[] CurrentStrategies { get; private set; }

//    public ReadOnlyTransform targetTF;
//    public Vector3? targetPos;


//    private void Notice()
//    {
//        targetTF = null;
//        targetPos = null;
//    }


//    public void CaculateNoticeVolume(Vector3 pos, Renderer renderer)
//    {
//        float vision = CaculateVisionRate(pos, renderer);
//        if (vision > 0)
//        {
//            Notice();
//        }
        
//    }

//    private float CaculateVisionRate(Vector3 pos,Renderer renderer)
//    {
//        ReadOnlyTransform tf = scripts.Cam.transform;
//        float distanc = (tf.position - pos).magnitude;

//        if (distanc > scripts.state.Insight*30) //? caculate Distance
//        {
//            print(distanc+":"+ scripts.state.Insight * 30);

//            return 0;
//        }

//        Ray ray = new Ray(tf.position, (tf.position - pos).normalized);

//        print(tf.rotation.eulerAngles + ":" + ray.direction);

//        Debug.DrawRay(tf.position, tf.rotation.eulerAngles);
//        Debug.DrawRay(tf.position, ray.direction);

//        if (Vector3.Dot(tf.rotation.eulerAngles, ray.direction) > scripts.state.Insight * 0.3f) //? caculate StartDirection
//        {
//            int shelter = Physics.RaycastAll(ray, distanc, VisionMask).Length;

//            float core = 100 / shelter * (1 - (distanc / (scripts.state.Insight * 30))) * 10;
//            return core;
//        }
//        else
//        {
//            return 0;
//        }
//    }


//    public void StategyAnalyze(StrategyValue[] strategies)
//    {
//        CurrentStrategies = strategies;
//    }
    


//    public bool hasTarget()
//    {
//        if (targetTF == null)
//        {
//            return false;
//        }
//        else
//        {
//            return true;
//        }
//    }


//}
