using UnityEngine;


public class SceneCam : MonoBehaviour
{
    public GameObject Target;
    private Camera cam;


    void Start()
    {
        cam = gameObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.LookAt(Target.transform, Vector3.up);
        cam.fieldOfView =Mathf.Clamp( (1/(gameObject.transform.position - Target.transform.position).magnitude / 2 * 3)*180,5,150);
    }
}
