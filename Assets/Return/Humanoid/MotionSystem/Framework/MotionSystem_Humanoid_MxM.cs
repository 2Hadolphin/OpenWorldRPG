using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MxM;
using MxMGameplay;
using TNet;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace Return.Motions
{

    public partial class MotionSystem_Humanoid /*MxM*/ : IMxMHandler
    {
        const string MxM = "MxM";

        protected MxMAnimator mxMAnimator;

        #region Resolver


        protected virtual void InstallResolver_MxM()
        {
            Resolver.RegisterModule<IMxMHandler>(this);
        }

        #endregion

        #region IMxMHandler

        public MxMAnimator mxm { get => mxMAnimator; }


        [RFC]
        public virtual void ExitEvent(int id)
        {
            if (isMine)
                tno.Send(nameof(ExitEvent), Target.OthersSaved, id);

            if (mxm.CheckEventPlaying(id))
                mxm.ForceExitEvent();
        }



        #region Sync FavorTag

        public virtual void AddFavourTag(ETags tag, float favour = -1)
        {
            mxm.AddFavourTags(tag);

            if (favour < 0)
                favour = mxm.FavourMultiplier;

            SetFavourTags(mxm.FavourTags, favour);
        }

        public virtual void RemoveFavourTags(ETags tag, float favour = -1)
        {
            mxm.RemoveFavourTags(tag);

            if (favour < 0)
                favour = mxm.FavourMultiplier;

            SetFavourTags(mxm.FavourTags, mxm.FavourMultiplier);
        }

        [RFC]
        public void SetFavourTags(ETags tags, float favour = 1f)
        {
            if (tno.isMine)
                tno.Send(nameof(SetFavourTags), Target.OthersSaved, tags, favour);

            mxm.SetFavourTags(tags, favour);
        }

        #endregion


        #region RequireTag

        public virtual void AddRequireTag(string tag)
        {
            mxm.AddRequiredTag(tag);

            SetRequireTag(mxm.RequiredTags);
        }

        public virtual void RemoveRequireTag(string tag)
        {
            mxm.RemoveRequiredTag(tag);

            SetRequireTag(mxm.RequiredTags);
        }

        [RFC]
        public void SetRequireTag(ETags tag)
        {
            if (tno.isMine)
                tno.Send(nameof(SetRequireTag), Target.OthersSaved, tag);
            else
                mxm.SetRequiredTags(tag);
        }


        #endregion


        #endregion




        public ScriptPlayable<MotionMatchingPlayable> MxMPlayable;

        #region Status

        public Vector3 AnimationDeltaPosition { get; protected set; }
        public Quaternion AnimationDeltaRotation { get; protected set; }

        #endregion

        protected virtual void Register_MxM()
        {
            InstanceIfNull(ref mxMAnimator);
            MxMPlayable = mxm.CreateMotionMatchingPlayable(ref MotionGraph);

            AnimMixer.AddInput(MxMPlayable, 0, 1);
        }

        protected virtual void Activate_MxM()
        {
            mxMAnimator.enabled = true;
        }


    }
}

