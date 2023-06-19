﻿using System;
using System.Collections.Generic;
/*
	14.01.19
		-add make string
		-add shuffle
		-add ToEnumerable
	26.06.19
		-add GetRandomPos
		-add Split
		-add Join 
    21.12.2020
    - fix Shuffle
*/
namespace Mkey {
    public static class CollectionExtension
    {
        /// <summary>
        /// Sum list members
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static int Sum(this List<int> list)
        {
            if (list == null || list.Count == 0) return 0;
            int res = 0;
            for (int i = 0; i < list.Count; i++)
            {
                res += list[i];
            }
            return res;
        }

        /// <summary>
        /// Make string from list members
        /// </summary>
        /// <param name="list"></param>
        /// <param name="div"></param>
        /// <returns></returns>
        public static string MakeString<T>(this List<T> list, string div)
        {
            if (list == null || list.Count == 0) return string.Empty;
            string res = "";
            for (int i = 0; i < list.Count; i++)
            {
                res += list[i].ToString();
                if (i != list.Count - 1)
                    res += div;
            }
            return res;
        }

        /// <summary>
        /// Check list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            if (list == null || list.Count == 0) return true;
            return false;
        }

        /// <summary>
        /// Check dictionary
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T1, T2>(this Dictionary<T1, T2> dict)
        {
            if (dict == null || dict.Count == 0) return true;
            return false;
        }

        /// <summary>
        /// Shuffle list members
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                int k = (UnityEngine.Random.Range(0, n) % n);
                n--;
                T val = list[k];
                list[k] = list[n];
                list[n] = val;
            }
        }

        /// <summary>
        /// Array to enumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        //https://stackoverflow.com/questions/1183083/is-it-possible-to-extend-arrays-in-c
        public static IEnumerable<T> ToEnumerable<T>(this Array target)
        {
            foreach (var item in target)
                yield return (T)item;
        }
    
	    public static T GetRandomPos<T>(this IList<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }
		
		/// <summary>
        /// Split list in two lists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static List<List<T>> Split<T>(this IList<T> list, int index)
        {
            List<List<T>> res = new List<List<T>>();

            int n = list.Count;
            List<T> l1 = new List<T>();
            List<T> l2 = new List<T>();
            if (index >= 0 && index < n)
            {
                for (int  i = 0;  i < n;  i++)
                {
                    if (i <= index) l1.Add(list[i]);
                    else l2.Add(list[i]);
                }
                
            }
            else if (index >= n)
            {
                l1 = new List<T>(list);
            }
            else if (index < 0)
            {
                l2 = new List<T>(list);
            }
            res.Add(l1);
            res.Add(l2);
            return res;
        }
		
		public static List<T> Join <T>(this IList<T> list,  IList<T> addList)
        {
            List<T> res = new List<T>(list);
            res.AddRange(addList);
            return res;
        }
	}
}
