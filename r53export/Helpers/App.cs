using System;

// ReSharper disable once CheckNamespace
namespace Pingfu.Route53Export
{
    internal static class App
    {
        /// <summary>
        /// 
        /// </summary>
        private static readonly ConsoleColor OriginalColor = Console.ForegroundColor;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exitCode"></param>
        internal static void Exit(int exitCode = 0)
        {
            Console.ForegroundColor = OriginalColor;
            Environment.Exit(exitCode);
        }
    }
}
