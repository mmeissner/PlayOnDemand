#region Licence
/****************************************************************
 *  Filename: QuickLeap_StringFormatting.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2017-8-2
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using LeapVR.Shared.Lib.Extensions;

namespace LeapVR.Shared.Lib.Helper
{
    public static partial  class QuickLeap
    {
        private static readonly object StaticRandomLock = new object();
        private static readonly Random StaticRandom = new Random();

        /// <summary>
        /// Formats number of bytes to string of format `123.7 MB` with given <see cref="precision"/>.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        public static string ToDiskSize(ulong bytes, int precision = 1)
        {
            string[] suf = { "B", "kB", "MB", "GB", "TB", "PB", "EB" };
            if (bytes == 0)
            {
                return "0 " + suf[0];
            }
            int place = System.Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), precision);
            return num.ToString(CultureInfo.InvariantCulture) + " " + suf[place];
        }

        /// <summary>
        /// Formats number of bytes to string of format `123.7 MB` with given <see cref="precision"/>.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="num">Numeric value of output, e.g. `123.7` for `123.7 MB`</param>
        /// <param name="unit">Unit string of output e.g. `MB` for `123.7 MB`</param>
        /// <param name="precision"></param>
        /// <returns></returns>
        public static string ToDiskSize(ulong bytes, out double num, out string unit, int precision = 1)
        {
            string[] suf = { "B", "kB", "MB", "GB", "TB", "PB", "EB" };
            if (bytes == 0)
            {
                num = 0d;
                unit = suf[0];
                return "0 " + suf[0];
            }
            int place = System.Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            unit = suf[place];
            num = Math.Round(bytes / Math.Pow(1024, place), precision);
            return num.ToString(CultureInfo.InvariantCulture) + " " + unit;
        }

        /// <summary>
        /// Formats number of bytes to string of format `123.7 MB` with given <see cref="precision"/>.
        /// </summary>
        /// <param name="byteCount"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        public static string ToDiskSize(long byteCount, int precision = 1)
        {
            string[] suf = { "B", "kB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
            {
                return "0 " + suf[0];
            }
            long bytes = Math.Abs(byteCount);
            int place = System.Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), precision);
            return (Math.Sign(byteCount) * num).ToString(CultureInfo.InvariantCulture) + " " + suf[place];
        }

        /// <summary>
        /// Gets relative path of <see cref="fullPath"/> given <see cref="baseDir"/>.
        /// Throws exception when operation cannot be completed.
        /// </summary>
        /// <param name="fullPath">Full path, e.g. `C:\Folder1\Folder2\file.txt`</param>
        /// <param name="baseDir">Base directory, e.g. `C:\Folder1\`</param>
        /// <returns>Relative path of <see cref="fullPath"/> to <see cref="baseDir"/>, e.g. `Folder2\file.txt`</returns>
        public static string GetRelativePath(string fullPath, string baseDir)
        {
            string relativePath;
            Exception exception;
            if (TryGetRelativePath(fullPath, baseDir, out relativePath, out exception))
            {
                return relativePath;
            }

            throw exception;
        }

        /// <summary>
        /// Tries get relative path of <see cref="fullPath"/> given <see cref="baseDir"/>.
        /// </summary>
        /// <param name="fullPath">Full path, e.g. `C:\Folder1\Folder2\file.txt`</param>
        /// <param name="baseDir">Base directory, e.g. `C:\Folder1\`</param>
        /// <param name="relativePath">Result value; Relative path of <see cref="fullPath"/> to <see cref="baseDir"/>, e.g. `Folder2\file.txt`</param>
        /// <returns>Boolean indicating success/failure</returns>
        public static bool TryGetRelativePath(string fullPath, string baseDir, out string relativePath)
        {
            Exception exception;
            return TryGetRelativePath(fullPath, baseDir, out relativePath, out exception);
        }

        /// <summary>
        /// Tries get relative path of <see cref="fullPath"/> given <see cref="baseDir"/>.
        /// </summary>
        /// <param name="fullPath">Full path, e.g. `C:\Folder1\Folder2\file.txt`</param>
        /// <param name="baseDir">Base directory, e.g. `C:\Folder1\`</param>
        /// <param name="relativePath">Result value; Relative path of <see cref="fullPath"/> to <see cref="baseDir"/>, e.g. `Folder2\file.txt`</param>
        /// <param name="exception">Exception that occured while trying to perform operation. Null if success.</param>
        /// <returns>Boolean indicating success/failure</returns>
        private static bool TryGetRelativePath(string fullPath, string baseDir, out string relativePath, out Exception exception)
        {
            relativePath = null;
            exception = null;

            if (string.IsNullOrEmpty(fullPath) || string.IsNullOrEmpty(baseDir))
            {
                exception = new ArgumentNullException(string.IsNullOrEmpty(fullPath) ? $"string.IsNullOrEmpty({nameof(fullPath)})" : $"string.IsNullOrEmpty({nameof(baseDir)})");
                return false;
            }

            baseDir = baseDir.TrimEnd('\\');

            if (string.Equals(fullPath, baseDir, StringComparison.CurrentCultureIgnoreCase))
            {
                relativePath = string.Empty;
                return true;
            }

            if (fullPath.Length <= baseDir.Length)
            {
                exception = new InvalidOperationException($"{nameof(fullPath)}.Length <= {nameof(baseDir)}.Length");
                return false;
            }

            if (fullPath[baseDir.Length] != '\\')
            {
                exception = new InvalidOperationException($"{nameof(fullPath)}[{nameof(baseDir)}.Length] != '\\\\'");
                return false;
            }

            relativePath = fullPath.Substring(baseDir.Length + 1);
            return true;
        }

        /// <summary>
        /// Formats <see cref="IEnumerable{T}"/> to string for debug purposes, using .ToString() method for each element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static string EnumerableToString<T>(IEnumerable<T> enumerable)
        {
            QuickLeap.AssertNotNull(enumerable);
            var clonedEnumerable = enumerable.ToList();

            int cnt = 0;
            var sb = new StringBuilder("{ ");
            foreach (var item in clonedEnumerable)
            {
                sb.Append(item != null ? item.ToString() : "null");
                cnt++;
                if (cnt != clonedEnumerable.Count)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(" }");

            return sb.ToString();
        }

        /// <summary>
        /// Formats collection of <see cref="Expression"/>s in format `() => SingleMember` to human readable string.
        /// Gets name of single member in every expression, invokes the expression to get return value of it and formats it to "{name} = `{value}`" format.
        /// Repeats that for all <see cref="Expression"/>s, then combines all results to one string using <see cref="EnumerableToString{T}"/>.
        /// </summary>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public static string ExpressionsToString(IEnumerable<Expression<Func<object>>> expressions)
        {
            QuickLeap.AssertNotNull(expressions);
            var copiedExpressions = expressions.ToArray();

            string ExpressionToString(Expression<Func<object>> exp)
            {
                var key = QuickLeap.GetFieldPropertyName(exp);
                var valueObj = exp.Compile().Invoke();
                var value = valueObj != null ? valueObj.ToString() : "<null>";

                return $"{key} = `{value}`";
            }

            return EnumerableToString(copiedExpressions.Select(ExpressionToString));
        }

        /// <summary>
        /// Generates new string of given length, with random characters (out of selected possible <see cref="characters"/>).
        /// Uses non cryptographically secure Random class.
        /// </summary>
        /// <param name="characters"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string RandomString(CharactersSet characters, int count)
        {
            if (characters == CharactersSet.None || count <= 0)
            {
                throw new ArgumentException($"Not valid input; characters = `{characters}`, count = `{count}`.");
            }

            string chars = "";

            if (characters.HasFlag(CharactersSet.LowerLetters))
            {
                chars = chars + "qwertyuiopasdfghjklzxcvbnm";
            }

            if (characters.HasFlag(CharactersSet.UpperLetters))
            {
                chars = chars + "QWERTYUIOPASDFGHJKLZXCVBNM";
            }

            if (characters.HasFlag(CharactersSet.Numbers))
            {
                chars = chars + "1234567890";
            }

            if (characters.HasFlag(CharactersSet.SpecialCharacters))
            {
                chars = chars + @"!@#$%^&*()_+-={}|[]\;':,./<>?~`";
            }

            var resultArray = new char[count];
            lock (StaticRandomLock)
            {
                for (int i = 0; i < count; i++)
                {
                    resultArray[i] = chars[StaticRandom.Next(chars.Length)];
                }
            }

            return new string(resultArray);
        }

        [Flags]
        public enum CharactersSet
        {
            None = 0,
            LowerLetters = 1 << 0,
            UpperLetters = 1 << 1,
            Numbers = 1 << 2,

            SpecialCharacters = 1 << 3,

            Alphanumerics = LowerLetters + UpperLetters + Numbers,
            All = LowerLetters + UpperLetters + Numbers + SpecialCharacters,
        }
    }
}
