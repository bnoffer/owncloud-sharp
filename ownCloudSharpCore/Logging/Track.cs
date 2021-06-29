using System;
using System.Diagnostics;
using owncloudsharp.Logging;

namespace owncloudsharp
{
    public static class Track
    {
        private static DateTime _lastTime = DateTime.Now;

        private static LogLevel LogLevel = LogLevel.Info;

        public static void Action(string Method = "", string Message = "")
        {
            if (((int)LogLevel & 0b0001) > 0) WriteDebug("[ACTION]", Method, Message);
        }


        public static void Command(string Method = "", string Message = "")
        {
            if (((int)LogLevel & 0b0001) > 0) WriteDebug("[COMMAND]", Method, Message);
        }


        public static void Event(string Method = "", string Message = "")
        {
            if (((int)LogLevel & 0b0001) > 0) WriteDebug("[EVENT]", Method, Message);
        }


        public static void Exception(string Method, Exception Ex)
        {
            try
            {
                if (((int)LogLevel & 0b1111) > 0) WriteDebug("[EXCEPTION]", Method, Ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public static void Failed(string Method = "", string Message = "")
        {
            if (((int)LogLevel & 0b0011) > 0) WriteDebug("[FAILED]", Method, Message);
        }


        public static void Info(string Method = "", string Message = "")
        {
            if (((int)LogLevel & 0b0011) > 0) WriteDebug("[INFO]", Method, Message);
        }


        public static void Navigation(string Method = "", string Message = "")
        {
            if (((int)LogLevel & 0b0001) > 0) WriteDebug("[NAVIGATION]", Method, Message);
        }


        public static void NoResponse(string Method = "", string Message = "")
        {
            if (((int)LogLevel & 0b0011) > 0) WriteDebug("[FAILED]", Method, "Response not sucessfull " + Message);
        }


        public static void State(string Method = "", string Message = "")
        {
            if (((int)LogLevel & 0b0011) > 0) WriteDebug("[STATE]", Method, Message);
        }


        public static void Success(string Method = "", string Message = "")
        {
            if (((int)LogLevel & 0b0011) > 0) WriteDebug("[SUCCESS]", Method, Message);
        }


        public static void Time(string Method = "", string Message = "")
        {
            if (((int)LogLevel & 0b1011) > 0) WriteDebug("[TIME]", Method, Message);
        }


        public static void Warning(string Method = "", string Message = "")
        {
            if (((int)LogLevel & 0b0011) > 0) WriteDebug("[WARNING]", Method, Message);
        }


        public static void WriteDebug(string Type, string Where = "", string Message = "")
        {
            try
            {
                TimeSpan _difference = DateTime.Now - _lastTime;

                string compiledMessage = $"Δt {_difference}" +
                                         $"\n\t\n\t{Type}\t{Where}" +
                                         $"\n\t " +
                                         $"\n\t\t{Message}" +
                                         $"\n\t \n\t_";

                Debug.WriteLine(compiledMessage);

                _lastTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
