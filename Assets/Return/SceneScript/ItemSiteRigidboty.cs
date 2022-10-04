using UnityEngine;

using Return;

public class ItemSiteRigidboty : MonoBehaviour
{
    //private RaycastHit Ranger;
    //public Vector3 vel;
    //public float timmer = 0f;
    //private float SensorRange=0f;
    //private float deadtime = 0f;
    //private void Start()
    //{
    //    CharacterRoot.AddComponent<Rigidbody>();
    //    CharacterRoot.GetComponent<Rigidbody>().drag = 0.7f;
    //    //print("Script On");
    //    SensorRange = CharacterRoot.GetComponentInChildren<Return.m_items.IItem>().BoundBox.max.magnitude + 0.1f;
    //}

    //private void Update()
    //{
    //    StateSensor();
    //}

    //private void StateSensor()
    //{
    //    bool contect = Physics.OnSensorUpdate(CharacterRoot.transform.position, Vector3.down, out Ranger, SensorRange);
    //    //Debug.DrawRay(CharacterRoot.transform.position, Vector3.down * 10f,Color.white);
    //    if (contect)
    //    {
    //        timmer += Time.deltaTime;
    //        vel = CharacterRoot.GetComponent<Rigidbody>().velocity;
    //        if (Mathf.Abs(vel.magnitude) <= 0.1f)
    //        {
    //            //print("Contect now");
    //        }
    //        else
    //        {
    //            timmer = 0f;
    //        }

    //    }
    //    else
    //    {
    //        //print("Null Contect ");
    //    }
    //    deadtime += Time.deltaTime;
    //    if (timmer >= 1.5f || deadtime > 5f)
    //    {
    //        Destroy(GetComponent<Rigidbody>());
    //        Destroy(this);
    //    }
    //}
      
}
