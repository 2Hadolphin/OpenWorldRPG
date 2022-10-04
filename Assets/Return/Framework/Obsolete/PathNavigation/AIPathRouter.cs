using Return;
using UnityEngine;
using UnityEngine.AI;

public class AIPathRouter : MonoBehaviour
{

   public NavMeshAgent agent { get; protected set; }

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    protected virtual void Start()
    {
        NavMeshAgent _agent=null;
        gameObject.InstanceIfNull(ref _agent);
        agent = _agent;
    }
    public virtual void Move(Vector3 pos)
    {
        agent.SetDestination(pos);
    }


}
