using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IPoolDebit
{
    void Spawn(Vector3 pos, Quaternion rot, bool enableRigidbody = false);
    void Spawn(Bounds Zone, bool enableRigidbody = false);
    void Spawn(Transform parent, Vector3 pos, Quaternion rot);
    void Retrieve();
}
