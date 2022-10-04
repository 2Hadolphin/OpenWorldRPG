using UnityEngine;
using System.Collections;
using Return.CentreModule;

using Return;
[RequireComponent(typeof(RocketLocator))]
public class RocketMotor : MonoBehaviour
{
    public Rigidbody rb { get; protected set; }

    public void Initializate()
    {
        if (gameObject.TryGetComponent<Rigidbody>(out Rigidbody _rb))
        {
            rb = _rb;
        }
        else
            rb = gameObject.AddComponent<Rigidbody>();


    }


    public Quaternion rotQ;
    public float rotSpeed = 0;
    public float applyForce = 0;
    public float applyVelocity = 0;
    //public string forcemode = "Force";

    protected Coroutine stroke;

    //initialization

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
            if (value == true)
            {
                if (stroke != null)
                    StopCoroutine(stroke);
                stroke = StartCoroutine(motor());
                //rb.isKinematic = false;
            }
            else
            {
                if (stroke != null)
                    StopCoroutine(stroke);
                stroke = null;
                //rb.isKinematic = true;
            }
        }
    }

    IEnumerator motor()
    {
        for(; ; )
        {
            yield return ConstCache.WaitForFixedUpdate;
            EngineFore(applyForce, applyVelocity);
            Rotation();
        }
    }

    public void EngineFore(float f ,float v)
    {
        rb.AddRelativeForce(transform.forward *f,ForceMode.Impulse);
        rb.velocity= transform.forward*(Mathf.Clamp((transform.forward * v).magnitude+rb.velocity.magnitude,0f,450f ));
    }

    private void Rotation()
    {
        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotQ, (1f + rotSpeed)));
    }

    
}
