//using NodeCanvas.Framework;
//using ParadoxNotion.Design;
//using ParadoxNotion;

//namespace Return.m_items.Weapons.Firearms
//{
//    public enum UTagCompareMethod
//    {
//        EqualTags,
//        /// <summary>
//        /// Atleast one tag.
//        /// </summary>
//        HasOneTag,
//        HasAllTags,
//        HasNoneTag,
//    }

//    [m_category("✫ Blackboard")]
//    public class CheckUTag : ConditionTask
//    {

//        public static string GetCompareString(UTagCompareMethod cm)
//        {
//            switch (cm)
//            {
//                case UTagCompareMethod.EqualTags:
//                    return " equals ";

//                case UTagCompareMethod.HasOneTag:
//                    return " has one tag of ";

//                case UTagCompareMethod.HasAllTags:
//                    return " match all tags ";

//                case UTagCompareMethod.HasNoneTag:
//                    return " has none tag of ";
//            }

//            return string.Empty;
//        }

//        public static bool Compare(UTag a, UTag b, UTagCompareMethod cm)
//        {
//            switch (cm)
//            {
//                case UTagCompareMethod.EqualTags:
//                    return a == b;

//                case UTagCompareMethod.HasOneTag:
//                    return b>0&& (a|b)==a;

//                case UTagCompareMethod.HasAllTags:
//                    return b>0&& a.HasFlag(b);

//                case UTagCompareMethod.HasNoneTag:
//                    return b==0 ||!a.HasFlag(b);
//            }

//            return true;
//        }

//        [BlackboardOnly]
//        public BBParameter<UTags> valueA;
//        public UTagCompareMethod checkType;
//        //public ParadoxNotion.CompareMethod checkType = ParadoxNotion.CompareMethod.EqualTo;
//        public BBParameter<UTags> valueB;

//        protected override string info
//        {
//            get 
//            {
//                return valueA.name + GetCompareString(checkType) + valueB.value.GetTag();
//                return valueA.name + GetCompareString(checkType) + valueB.value.Tag; 
//            }
//        }



//        protected override bool OnCheck()
//        {
//            return Compare(valueA.value, valueB.value, checkType);
//        }
//    }
//}