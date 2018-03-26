// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ClassifyBot
{
    public static class StringExtensions
    {
        public static string F(this string formatString, params object [] o)
        {
            return string.Format(formatString, o);
        }

        public static bool IsEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static bool IsNotEmpty(this string s)
        {
            return !string.IsNullOrEmpty(s);
        }

        public static string ToOneCharString(this char c)
        {
            return new string(c, 1);
        }

        public static string ToStringInvariant<T>(this T value)
        {
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static string ToStringLocal<T>(this T value)
        {
            return Convert.ToString(value, CultureInfo.CurrentCulture);
        }

        public static string FormatInvariant(this string value, params object[] arguments)
        {
            return string.Format(CultureInfo.InvariantCulture, value, arguments);
        }

        public static string FormatLocal(this string value, params object[] arguments)
        {
            return string.Format(CultureInfo.CurrentCulture, value, arguments);
        }

        public static string Spaces(this int value)
        {
            return new string(' ', value);
        }

        public static bool EqualsOrdinal(this string strA, string strB)
        {
            return string.CompareOrdinal(strA, strB) == 0;
        }

        public static bool EqualsOrdinalIgnoreCase(this string strA, string strB)
        {
            return string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static int SafeLength(this string value)
        {
            return value == null ? 0 : value.Length;
        }

        public static string JoinTo(this string value, params string[] others)
        {
            var builder = new StringBuilder(value);
            foreach (var v in others)
            {
                builder.Append(v);
            }
            return builder.ToString();
        }

        public static bool IsBooleanString(this string value)
        {
            return value.Equals("true", StringComparison.OrdinalIgnoreCase)
                || value.Equals("false", StringComparison.OrdinalIgnoreCase);
        }

        public static bool ToBoolean(this string value)
        {
            return value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        public static string StripUTF8BOM(this string s)
        {
            string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            return s.Replace(_byteOrderMarkUtf8, string.Empty);

        }

        public static bool IsAlphaNumeric(this string value)
        {
            return value.All(c => Char.IsLetterOrDigit(c));
        }

        public static string ToAlphaNumeric(this string s)
        {
            char[] chars = s.Where(c => Char.IsLetterOrDigit(c)).ToArray();
            return new String(chars);
        }

        public static string WithDllExt(this string s)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return s + ".dll";
            }
            else
            {
                return s;
            }
        }
    }
}