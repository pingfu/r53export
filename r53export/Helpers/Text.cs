using System;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Pingfu.Route53Export
{
    internal static class Text
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        internal static byte[] HexToByteArray(this string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static string EscapeSpecialCharacters(this string data)
        {
            return data
                .Replace("\\052", "*")
                .Replace("\\100", "@");
        }
    }
}
