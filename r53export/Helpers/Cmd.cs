﻿using System;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Pingfu.Route53Export
{
    internal static class Cmd
    {
        /// <summary>
        /// 
        /// </summary>
        internal const ConsoleColor Red = ConsoleColor.Red;

        /// <summary>
        /// 
        /// </summary>
        internal const ConsoleColor White = ConsoleColor.White;

        /// <summary>
        /// 
        /// </summary>
        internal const ConsoleColor Yellow = ConsoleColor.Yellow;

        /// <summary>
        /// 
        /// </summary>
        private static readonly IFormatProvider FormatProvider = Thread.CurrentThread.CurrentCulture;

        /// <summary>
        /// 
        /// </summary>
        internal static void WriteLine()
        {
            Write(Console.ForegroundColor, "\n");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        internal static void WriteLine(string format, params object[] args)
        {
            Write(Console.ForegroundColor, String.Format(FormatProvider, format, args) + "\n");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        internal static void WriteLine(ConsoleColor color, string format, params object[] args)
        {
            Write(color, String.Format(FormatProvider, format, args) + "\n");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        internal static void Write(string format, params object[] args)
        {
            Write(Console.ForegroundColor, String.Format(FormatProvider, format, args));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        internal static void Write(ConsoleColor color, string format, params object[] args)
        {
            var originalColor = Console.ForegroundColor;

            Console.ForegroundColor = color;
            Console.Write(String.Format(FormatProvider, format, args));
            Console.ForegroundColor = originalColor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        internal static void WriteInPlace(string format, params object[] args)
        {
            // carriage return to start of current line
            Console.Write("\r");
            
            // clear text on the current line
            Console.Write(new string(' ', Console.WindowWidth -1));

            // carriage return to start of current line
            Console.Write("\r");

            // write the text
            Console.Write(String.Format(FormatProvider, format, args));
        }
    }
}