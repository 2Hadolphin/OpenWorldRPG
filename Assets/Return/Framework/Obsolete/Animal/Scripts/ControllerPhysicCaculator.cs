using UnityEngine;


public class ControllerPhysicCaculator : MonoBehaviour
{

    public BasicController controller;

    public enum CharactorCollider { Sphere, Capsule, Custom }
    public CharactorCollider PhysicColliderType;

    [Tooltip("Is Using Capsule or Custome ColliderBounds ?")]
    public float Radius;
    public Vector3 Size;

    [SerializeField]
    private Transform GroundedTF=null;


    private float GroundedReCastDistance=0;

    public MeshCollider TargetCollider;

    private SphereCollider physiccollider;
    [SerializeField]
    private LayerMask TargetLayer;

    [SerializeField]
    private int GroundedDistance = 1;
    [SerializeField]
    private float GroundedIteration=5;
    private float nextGroundedDistance=20;

    private void OnEnable()
    {
        //InstanceCollider(PhysicColliderType);
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }
    private void Update()
    {

    }

    private void FixedUpdate()
    {
        CheckGrounded();
        /*
        if (CheckGrounded())
        {
            print("OK");
        }
        */
    }

    private bool CheckGrounded()
    {

        if (Physics.Raycast(GroundedTF.position, Vector3.down, out RaycastHit hit, nextGroundedDistance, TargetLayer))
        {
            controller.Move(Vector3.down * (hit.distance - 0.1f));
            nextGroundedDistance = 1;
            return true;
        }
        else
        {
            Radius = 1;

            controller.Move(Vector3.down * 20);
            nextGroundedDistance = 20;
            GroundedReCastDistance += nextGroundedDistance * 2;
            return ReCheckGrounded();
        }
    }

    private bool ReCheckGrounded()
    {
        for(int i = 0; i < GroundedIteration; i++)
        {
            if (Physics.Raycast(GroundedTF.position + Vector3.up * GroundedReCastDistance, Vector3.down, out RaycastHit hit, nextGroundedDistance + GroundedReCastDistance*2, TargetLayer))
            {
                controller.Move(Vector3.down * (hit.distance - GroundedReCastDistance - 0.1f));
                nextGroundedDistance = 1;
                return true;
            }
            else
            {
                Radius = 1;

                controller.Move(Vector3.down * 20);
                nextGroundedDistance = 20;
                GroundedReCastDistance += nextGroundedDistance * 2;
            }
        }
        return false;
    }




    private void InstanceCollider(CharactorCollider colliderType)
    {
        switch ((int)colliderType)
        {
            case 0:
                physiccollider = gameObject.AddComponent<SphereCollider>();
                physiccollider.radius = Radius;
                break;
            case 1:

                break;
            case 2:

                break;
            default:
                break;
        }
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        switch ((int)PhysicColliderType)
        {
            case 0:
                if(Radius>0)
                Gizmos.DrawWireSphere(this.gameObject.transform.position, Radius);
                break;
            case 1:
                if(Size.magnitude>0)
                Gizmos.DrawWireCube(this.gameObject.transform.position, Size);
                break;
            case 2:
                if(TargetCollider!=null)
                Gizmos.DrawWireMesh(TargetCollider.sharedMesh);
                break;
            default:
                break;
        }

    }
}
