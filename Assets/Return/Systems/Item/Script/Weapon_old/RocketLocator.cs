using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UIElements;
using Return.CentreModule;

using Return;
[RequireComponent(typeof(ProjectTileManager))]
public class RocketLocator : MonoBehaviour
{
    private ProjectTileManager manager;
    private RocketMotor motor;

    //load-in info
    private Vector3 CDN;
    [SerializeField]
    private Transform Finalltarget=null;


    private float Totaldistance =0f;
    private float rotSpeed = 0f;
    private float rotclamp = 0f;
    [SerializeField]
    private float xVel = 0f;
    private float yVel = 0f;
    [SerializeField]
    private float zVel = 0f;
    private float fuel;

    public enum MissileState { Eject,Raise, Cruise,Dive,Target }
    public MissileState state { get; protected set; }
    protected Coroutine stroke;

    //initialization
    public void SetGuid(Transform target)
    {
        Finalltarget = target;
    }

    public bool Active 
    {
        get
        {
            if (stroke == null)
                return false;
            else
                return true;
        }
        set
        {
            if (value==true)
            {
                if (stroke != null)
                    StopCoroutine(stroke);
                else
                    fuel = manager.getData.Fuel*1;

                stroke = StartCoroutine(loacte());
            }
            else
            {
                if (stroke != null)
                    StopCoroutine(stroke);
                stroke = null;
            }
        }
    }

    IEnumerator loacte() 
    {
        for(; ; )
        {
            yield return ConstCache.WaitForFixedUpdate;
            fuel -= 1 * ConstCache.fixeddeltaTime;
            if (fuel < 0)
                manager.Explode();
            switch (state)
            {
                case MissileState.Eject:
                    Level_Eject();
                    break;
                case MissileState.Raise:
                    Level_Raise();
                    break;
                case MissileState.Cruise:
                    Level_Cruise();
                    break;
                case MissileState.Dive:
                    Level_Dive();
                    break;
                case MissileState.Target:
                    Level_Target();
                    break;
            }
        }

    }

    public void Initializate()
    {
        manager = GetComponent<ProjectTileManager>();
        motor = GetComponent<RocketMotor>();
    } 

    private void Level_Eject()
    {
        FinishState();
    }


    private void Level_Raise() //Climbing up
    {

        rotSpeed = rotSpeed + 0.1f;
        motor.rotSpeed = rotSpeed;
        Contact(0.2f);
        applyCalculate(manager.getData.reachTime[0]);
        manager.DrawTargetUI(false);
    }


    private void Level_Cruise()   //Guiding
    {
        CDN = new Vector3
            (
                Finalltarget.position.x,
                0f,
                Finalltarget.position.z
            );
        rotSpeed = rotSpeed + 0.1f;
        motor.rotSpeed = rotSpeed;
        Contact(0.01f);
        applyCalculate(manager.getData.reachTime[0]);
        manager.DrawTargetUI(false);

    }

    private void Level_Dive()
    {
        FinishState();
    }

    private void Level_Target()
    {
        FinishState();
        manager.DrawTargetUI(false);
    }


    private void LoadParameter()
    {
        Totaldistance = (CDN - transform.position).magnitude;
 
        xVel = motor.rb.velocity.x;
        zVel = motor.rb.velocity.z;
        CDN = new Vector3(xVel * 5, 100f, zVel * 5);

        if (Finalltarget.transform.position.y < 100f)// rows battle platten
        {
            yVel =100f;
        }
        else
        {
            yVel = Finalltarget.transform.position.y;
        }

    }

    private void applyCalculate(int reachtime) //caculate force
    {
        
        //rotation
        motor.rotQ =Quaternion.Lerp(motor.rotQ, Quaternion.LookRotation(CDN - transform.position),Time.fixedDeltaTime*rotSpeed);

        //position
        motor.applyVelocity =Mathf.Clamp(
                (((CDN - transform.position).magnitude / (reachtime))+(rotSpeed)),0,manager.getData.MaxSpeed
                                                 );
    }

    private void Contact(float ExplosiveRange)
    {
        if ((CDN - transform.position).magnitude / Totaldistance < ExplosiveRange || (CDN - transform.position).sqrMagnitude< 400)
        {
            FinishState();
            rotSpeed = 0f;
        }
    }

    private void FinishState()
    {
        switch (state)
        {
            case MissileState.Eject:
                state = MissileState.Raise;
                break;
            case MissileState.Raise:
                state = MissileState.Cruise;
                break;
            case MissileState.Cruise:
                state = MissileState.Dive;
                break;
            case MissileState.Dive:
                state = MissileState.Target;
                break;
            case MissileState.Target:
                manager.Explode();
                return;
        }
        //print("Mode " + state + " Online");
        LoadParameter();

    }




}


 

