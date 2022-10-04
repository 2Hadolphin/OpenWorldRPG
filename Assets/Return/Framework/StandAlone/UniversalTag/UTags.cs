using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

namespace Return
{
    /// <summary>
    /// Container of UTag which has tag definitionl
    /// </summary>
    [Serializable]
    [HideLabel]
    public struct UTags: IUTags,IEquatable<UTags>,IEquatable<UTagPicker>
    {

#if UNITY_EDITOR

        [HideInInspector]
        public bool HideDefinition;
#endif

        [SerializeField]
        public UniversalTagDefinition Definition;

        [SerializeField]
        [HideInInlineEditors]
        public UTag Tag;


        #region Interface

        UniversalTagDefinition IUTags.Definition => Definition;

        UTag IUTags.GetTag => Tag;

        #endregion

        public string GetTag()
        {
            var firstTag=Tag.IndexOfFirstOrDefault();

            if (firstTag > 0)
                return Definition[firstTag - 1];
            else
                return "None";
        }

        public override string ToString()
        {
            return GetTag();
        }

        public IEnumerable<string> GetAllTags()
        {
            var indexs = Tag.GetAllTagsIndex();

            foreach (var index in indexs)
                yield return Definition[index];
        }

        public bool Equals(UTagPicker other)
        {
            return Tag == other.Tag;
        }

        public bool Equals(UTags other)
        {
            return Definition==other.Definition && Tag == other.Tag;
        }

        public static implicit operator UTag(UTags uTags)
        {
            return uTags.Tag;
        }

        public static implicit operator string(UTags uTags)
        {
            return uTags.GetTag();
        }
    }
}

