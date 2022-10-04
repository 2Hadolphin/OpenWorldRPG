using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return;
using System;
using MxM;
using KinematicCharacterController;
using UnityEngine.Events;
using HoaxGames;
using Return.Framework.PhysicController;

[DefaultExecutionOrder(ExecuteOrderList.IndependentChildModules)]
public class StepSoundManager : BaseComponent
{
    [SerializeField]
    public List<AudioPreset> Presets;

    private void Start()
    {
        InstanceIfNull(ref Player);
        Controller = InstanceIfNull<ICharacterControllerMotor,CharacterControllerMotor>();
        InstanceIfNull(ref MxM);


        MxM.OnRightFootStepStart.AddEvent(OnRightStep);
        MxM.OnLeftFootStepStart.AddEvent(OnLeftStep);

        var animator = MxM.UnityAnimator;
        RightFoot.Bone = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        LeftFoot.Bone = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
    }

    ICharacterControllerMotor Controller;
    MxMAnimator MxM;
    AudioSource Player;

    public float GroundedLimit = 0.1f;

    Step RightFoot;
    Step LeftFoot;

    struct Step
    {
        public Transform Bone;
        public bool OnStep;
    }


    private void FixedUpdate()
    {
        //FootGrounding(ref RightLeg);
        //FootGrounding(ref LeftLeg);
    }

    public virtual void OnStepStart(IKResult result)
    {
        if(result.isGlued&& result.primaryHitTransform && result.primaryHitTransform.TryGetComponent<Collider>(out var col))
        {
            var mat = col.sharedMaterial;
            if (mat)
                foreach (var set in Presets)
                {
                    if (mat == set.Material)
                    {
                        Player.PlayOneShot(set.Clips.Random());
                        return;
                    }
                }

            Player.PlayOneShot(Presets.Random().Clips.Random());
        }
    }


    void FootGrounding(ref Step step)
    {
        if (!step.Bone)
            return;

        if (!step.OnStep&&Physics.Raycast(step.Bone.position, -step.Bone.up, out var hit,0.1f))
        {
            var mat = hit.collider.sharedMaterial;
            if (mat)
                foreach (var set in Presets)
                {
                    if (mat == set.Material)
                    {
                        Player.PlayOneShot(set.Clips.Random());
                        return;
                    }

                }
            step.OnStep = true;
        }
        else
        {
            step.OnStep = false;
        }
    }

    public virtual void OnRightStep(FootStepData data)
    {
        PlayAudio(RightFoot.Bone);
        //Debug.Log(data.Type);
    }

    public virtual void OnLeftStep(FootStepData data)
    {
        PlayAudio(LeftFoot.Bone);
        //Debug.Log(data.Type);
    }

    protected void PlayAudio(Transform tf)
    {
        if (!tf)
            return;

        if(Physics.Raycast(tf.position,-tf.up,out var hit))
        {
            if (hit.distance > GroundedLimit)
            {

                //Debug.Log(hit.distance+"**"+hit.collider);
                return;
            }

            var mat = hit.collider.sharedMaterial;
            if(mat)
                foreach (var set in Presets)
                {
                    if (mat == set.Material)
                    {
                        Player.PlayOneShot(set.Clips.Random());
                        return;
                    }

                }
        }
    }

    [Serializable]
    public struct AudioPreset
    {
        public PhysicMaterial Material;
        public List<AudioClip> Clips;
    }
}
