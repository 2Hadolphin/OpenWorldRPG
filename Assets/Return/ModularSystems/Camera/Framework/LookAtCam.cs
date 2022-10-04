using UnityEngine;

public class LookAtCam : MonoBehaviour
{
    

    void Update()
    {
        gameObject.transform.LookAt(Camera.main.transform, Vector3.up);
    }
}
