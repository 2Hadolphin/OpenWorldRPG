using MxM;

namespace Return.Motions
{
    /// <summary>
    /// Interface to access MxM animator and proccess network sync.
    /// </summary>
    public interface IMxMHandler
    {
        /// <summary>
        /// Mxm motion matching animator.
        /// </summary>
        MxMAnimator mxm { get; }

        /// <summary>
        /// Exit mxm event and sync.
        /// </summary>
        void ExitEvent(int id);

        /// <summary>
        /// Sync require tag.
        /// </summary>
        void SetRequireTag(ETags tag);
        void AddRequireTag(string tag);
        void RemoveRequireTag(string tag);


        /// <summary>
        /// Sync favour tag.
        /// </summary>
        void SetFavourTags(ETags tags, float favour = 1f);
        void AddFavourTag(ETags tag, float favour = -1);
        void RemoveFavourTags(ETags tag, float favour = -1);


    }
}

