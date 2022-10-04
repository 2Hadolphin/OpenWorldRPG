using System;
using System.Collections.Generic;
using UnityEngine;

namespace Return
{


    //============================================================================================
    /**
    *  @brief Static class used to easily manage UniversalTag flags in an easily readable way.
    *         
    *********************************************************************************************/
    public static class UniversalTagExtension
    {
        public static UTag SetTag(this UTag _source, UTag _tag)
        {
            return _source | _tag;
        }

        public static UTag UnsetTag(this UTag _source, UTag _tag)
        {
            return _source & (~_tag);
        }

        public static bool HasTags(this UTag _source, UTag _tag)
        {
            return (_source & _tag) == _tag;
        }

        public static UTag ToggleTag(this UTag _source, UTag _tag)
        {
            return _source ^ _tag;
        }

        public static UTag FirstOrDefault(this UTag tags)
        {
            var tagArray = Enum.GetValues(typeof(UTag));

            if ((tags & (tags - 1)) == 0)
            {
                return tags;//(UTag)Array.IndexOf(tagArray, tags);
            }
            else
            {
                var index = 0;
                foreach (UTag tag in tagArray)
                {
                    if(index>0)
                        if ((tags | tag) == tags)
                            return tag;

                    index++;
                }
            }

            return UTag.None;
        }

        public static int IndexOfFirstOrDefault(this UTag tags)
        {
            var tagArray = Enum.GetValues(typeof(UTag));

            if ((tags & (tags - 1)) == 0)
            {
                var index= Array.IndexOf(tagArray, tags);

                //if (index > 0)
                //    index --;
                //Debug.Log((UTag)(index==0?index:Math.Pow(2,index-1)) + " ** " + tags);
                return index;
            }
            else
            {
                var index = 0;

                foreach (UTag tag in tagArray)
                {
                    if (index > 0)
                        if ((tags | tag) == tags)
                            return index;

                    index++;
                }
            }

            return 0;
        }


        public static IEnumerable<int> GetAllTagsIndex(this UTag tags)
        {
            var tagArray = Enum.GetValues(typeof(UTag));
            int index = 0;

            foreach (UTag tag in tagArray)
            {
                if (tag > 0)
                {
                    if ((tags | tag) == tags)
                        yield return index;
                }

                index++;
            }
        }
    }


}
