using System;
using System.IO;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Pingfu.Route53Export
{
    internal static class Log
    {
        /// <summary>
        /// 
        /// </summary>
        private static readonly IFormatProvider FormatProvider = Thread.CurrentThread.CurrentCulture;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        internal static void Write(TextWriter file, string format, params object[] args)
        {
            file.WriteLine(string.Format(FormatProvider, format, args));
        }
    }
}
