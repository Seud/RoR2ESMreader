using System;

namespace ESMReader
{
    class Logger
    {
        public static void Log(string message)
        {
            Console.WriteLine(message);
        }

        public static void Warn(string message)
        {
            Console.WriteLine("Warning : " + message);
        }

        public static void Error(string message)
        {
            Console.WriteLine("ERROR : " + message);
            Environment.Exit(1);
        }
    }
}
