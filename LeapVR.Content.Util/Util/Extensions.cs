#region Licence
/****************************************************************
 *  Filename: Extensions.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Date          2026-05-19
 *  Copyright (c) 2026 Martin Meissner.
 *                Released under the Apache License 2.0 as part of
 *                the open-source PlayOnDemand release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using LeapVR.Content.Util.Archive;

namespace LeapVR.Content.Util.Util
{
    internal static class FilePathNameExtensions
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static ContentDirectory ToContentDirectory(this DirectoryInfo directoryInfo)
        {
            return new ContentDirectory(directoryInfo, directoryInfo);
        }
    }
    public static class TypeLoaderExtensions
    {
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }

    public static class DictonaryExtensions
    {
        public static void MergeWithoutDuplicates<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> dictionare)
        {
            Dictionary<TKey, TValue>[] dic = new Dictionary<TKey, TValue>[2];
            dic[0] = dictionare;
            dic[1] = dictionary;
            var retval = dic.SelectMany(dict => dict).ToDictionary(pair => pair.Key, pair => pair.Value);
            dictionary.Clear();
            foreach (var pair in retval)
            {
                dictionary.Add(pair.Key,pair.Value);
            }
        }
    }
    
}
