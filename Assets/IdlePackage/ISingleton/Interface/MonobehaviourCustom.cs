using System.Collections.Generic;
using UnityEngine;

namespace Hiker.Idle
{
    public class MonobehaviourCustom : MonoBehaviour
    {
        public static void DebugLog(string name, string Messeger)
        {
            Debug.Log(formatString(name, Messeger));
        }
        public static void DebugLog(string name, string Messeger, string Color = "Blue", string Tag = "Log")
        {
            Debug.Log(formatStringColorTag(name, Messeger, Color, Tag));
        }

        public static void DebugWarnning(string name, string Messeger)
        {
            Debug.LogWarning(formatString(name, Messeger));
        }

        public static void DebugError(string name, string Messeger)
        {
            Debug.LogError(formatString(name, Messeger));
        }

        private static string formatString(string name, string Messeger)
        {
            return string.Format("{0}: {1}", name, Messeger);
        }
        private static string formatStringColorTag(string name, string Messeger, string Color, string Tag)
        {
            return string.Format("{0}: [<Color={2}>{3}</Color>]: {1}", name, Messeger, Color, Tag);
        }

        #region Function Static ConverTo

        public static List<T> ConverToListFromArray<T>(T[] input)
        {
            List<T> result = new List<T>();
            foreach (var item in input)
            {
                result.Add(item);
            }

            return result;
        }

        public static List<T> ConverToListFromDic<T>(Dictionary<string, T> input)
        {
            List<T> result = new List<T>();
            foreach (var item in input)
            {
                result.Add(item.Value);
            }

            return result;
        }

        #endregion
    }
}
