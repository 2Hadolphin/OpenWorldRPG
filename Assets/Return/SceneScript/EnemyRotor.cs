using UnityEngine;
using System;

public class EnemyRotor : MonoBehaviour
{
    
    public int movspeed = 0;

    private void OnEnable()
    {
        setting();

    }



    private void Start()
    {
  
    }

    
    private void Update()
    {
        around();
    }

    //initialization
    void setting()
    {
        movspeed = UnityEngine.Random.Range(-5, 5);
    }

    //moving
    private void around()
    {
        transform.RotateAround(Vector3.zero,Vector3.up , movspeed * Time.deltaTime);

    }

    //tookdamage
    public void Dead()
    {
        gameObject.GetComponentInChildren<BoxCollider>().isTrigger = false;
        Destroy(gameObject, 10);
        if(!gameObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb=gameObject.AddComponent<Rigidbody>();
        }

        rb.AddForce(Vector3.down, ForceMode.Force);
        rb.isKinematic = false;
        setting();

    }


}
