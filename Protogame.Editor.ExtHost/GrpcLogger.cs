using Grpc.Core.Logging;
using System;

namespace Protogame.Editor.ExtHost
{
    public class GrpcLogger : ILogger
    {
        public void Debug(string message)
        {
            System.Console.Error.WriteLine("GRPC DEBUG: " + message);
        }

        public void Debug(string format, params object[] formatArgs)
        {
            System.Console.Error.WriteLine("GRPC DEBUG: " + string.Format(format, formatArgs));
        }

        public void Error(string message)
        {
            System.Console.Error.WriteLine("GRPC ERROR: " + message);
        }

        public void Error(string format, params object[] formatArgs)
        {
            System.Console.Error.WriteLine("GRPC ERROR: " + string.Format(format, formatArgs));
        }

        public void Error(Exception exception, string message)
        {
            System.Console.Error.WriteLine("GRPC ERROR: " + exception.ToString());
        }

        public ILogger ForType<T>()
        {
            return this;
        }

        public void Info(string message)
        {
            System.Console.Error.WriteLine("GRPC INFO: " + message);
        }

        public void Info(string format, params object[] formatArgs)
        {
            System.Console.Error.WriteLine("GRPC INFO: " + string.Format(format, formatArgs));
        }

        public void Warning(string message)
        {
            System.Console.Error.WriteLine("GRPC WARNING: " + message);
        }

        public void Warning(string format, params object[] formatArgs)
        {
            System.Console.Error.WriteLine("GRPC WARNING: " + string.Format(format, formatArgs));
        }

        public void Warning(Exception exception, string message)
        {
            System.Console.Error.WriteLine("GRPC WARNING: " + exception.ToString());
        }
    }
}
