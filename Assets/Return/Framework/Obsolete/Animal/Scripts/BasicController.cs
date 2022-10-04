using System.Collections;
using UnityEngine;
using System;


//[RequireComponent(typeof(CharacterController))]
public class BasicController : MonoBehaviour
{
    [Header("Normal Setting")]
    [SerializeField]
    private int SN = 0;

    public enum PhysicType { MathModel,PathMove,Caculator,RigMove}
    public PhysicType OperatingStatus;

    [SerializeField]
    private ControllerScripts scripts;

    [SerializeField]
    public GameObject alpha;

    public SphereCollider sphereCollider;
    
    public int gooo =1;

    private void Update()
    {
    }

    private void Initialization()
    {
        SetPhysicMethod();

    }

    private void SetPhysicMethod()
    {
        switch ((int)OperatingStatus)
        {
            case 0:
                break;
            case 1:
                break;
            case 2:
                scripts.GetPhysicScriptSlot.gameObject.AddComponent<ControllerPhysicCaculator>().controller=this;
                break;
            case 3:
                break;
            default:
                break;

        }
    }


    
    public void Move(Vector3 go) 
    {
        gameObject.transform.localPosition += go;
    }

    public void Move(float go)
    {

    }

    IEnumerator LinearMove(float Direction,float ElevationAngle,float SpeedStart,float DecayRate)
    {
        yield break;
    }

    IEnumerator SetTest()
    {
        
        yield return new WaitForSeconds(5);
        sphereCollider.isTrigger = true;
        yield return new WaitForSeconds(5);
        sphereCollider.enabled = false;
    }

    private void Start()
    {
        StartCoroutine(SetTest());
    }
}
