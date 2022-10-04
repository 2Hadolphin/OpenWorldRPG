using UnityEngine;
using System.Text.RegularExpressions;
namespace Return
{
    public static class mTextUtility
    {
        public const string TitlecaseLetter= @"[A-Z][a-z]*|[a-z]+";
        public static string CombinePattern(string patternA, string patternB)
        {
            Debug.Log(patternA+patternB);
            return string.Format("@{0}+{1}", patternA.Replace("@", ""), patternB.Replace("@", "")) ;
        }
        public static string[] Depart(string input, string pattern)
        {
            var results = Regex.Matches(input.Replace("_","."), pattern);

            var value =new string[results.Count];
            var index = 0;
            foreach (Match match in results)
            {
                value[index] = match.Value;
                index++;
            }
        

            return value;
        }
    }
}