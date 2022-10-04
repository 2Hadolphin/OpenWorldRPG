using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Use this rig to control first person hands and top config
/// </summary>
public class FirsetPersonHandleRig : RigConstraint<FPHandleJob, FPHandleJobData, FPHandleBinder>
{

    private void Awake()
    {
        //Handle=new GameObject("Handle").transform;
        //Handle.parent = transform;
        //Handle.position = Vector3.zero;
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        
    }
}

public interface IFPHandleJob : IWeightedAnimationJob
{

}

//[BurstCompile]
public struct FPHandleJob : IWeightedAnimationJob
{
    public AnimationJobCache cache;
    public UnityEngine.Animations.Rigging.CacheIndex _ClampRotation;
    public TransformStreamHandle Upchest;

    public FloatProperty jobWeight { get; set; }
    public void ProcessAnimation(AnimationStream stream)
    {
        //if (jobWeight.value.GetFloat(stream) == 0)
        //    return;
        Debug.Log(jobWeight.Get(stream));
        Upchest.SetLocalRotation(stream, cache.Get<Quaternion>(_ClampRotation));
        Upchest.Resolve(stream);

    }

    public void ProcessRootMotion(AnimationStream stream)
    {

    }
}

public interface IFPHandleData : IAnimationJobData
{
    string weightProperty { get; }
}

[System.Serializable]
public struct FPHandleJobData : IFPHandleData
{
    [SyncSceneToStream, SerializeField, Range(0f, 1f)] float m_weight;
    public string weightProperty => ConstraintsUtils.ConstructConstraintDataPropertyName(nameof(m_weight));

    [Range(0,1f)]
    public float HandleWeight ;
    [SerializeField]
    [Tooltip("Upchest or root of arms.")]
    public Transform HandleBone;

    public Vector3 ClampRotation;

    public bool IsValid()
    {
        return true;
    }

    public void SetDefaultValues()
    {
        HandleWeight = 1f;
    }
}


public class FPHandleBinder : AnimationJobBinder<FPHandleJob, FPHandleJobData> 
{
    public FPHandleBinder() { }

    

    public override FPHandleJob Create(Animator animator, ref FPHandleJobData data, Component component)
    {
        Debug.Log(component.GetType());

        var job = new FPHandleJob();

        job.Upchest = animator.BindStreamTransform(data.HandleBone);
        job.jobWeight = FloatProperty.Bind(animator, component, data.weightProperty);

        var cacheBuilder = new AnimationJobCacheBuilder();
        job._ClampRotation = cacheBuilder.Add(data.ClampRotation);
        job.cache = cacheBuilder.Build();

        return job;
    }

    public override void Destroy(FPHandleJob job)
    {
        job.cache.Dispose();
    }

    public override void Update(FPHandleJob job, ref FPHandleJobData data)
    {

        job.cache.Set(Quaternion.Euler(data.ClampRotation), job._ClampRotation);
    }
}