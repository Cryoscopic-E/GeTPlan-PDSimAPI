using Proto;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeTPlanModel
{
    [System.Serializable]
    public enum LogMessageLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }
    [System.Serializable]
    public class GeTLogMessage
    {
        public LogMessageLevel LogMessageType { get; set; }
        public string Message { get; set; }

        public GeTLogMessage(LogMessageLevel logMessageType, string message)
        {
            this.LogMessageType = logMessageType;
            this.Message = message;
        }

        public override string ToString()
        {
            return $"[{LogMessageType.ToString().ToUpper()}]: {Message}";
        }

        public static LogMessageLevel GetLogMessageType(LogMessage.Types.LogLevel level)
        {
            switch (level)
            {
                
                case LogMessage.Types.LogLevel.Debug:
                    return LogMessageLevel.Debug;
                default:
                case LogMessage.Types.LogLevel.Info:
                    return LogMessageLevel.Info;
                case LogMessage.Types.LogLevel.Warning:
                    return LogMessageLevel.Warning;
                case LogMessage.Types.LogLevel.Error:
                    return LogMessageLevel.Error;
            }
        }
    }

    public class LogMessageModelFactory : IGeTModelFactory<GeTLogMessage, LogMessage>
    {
        public GeTLogMessage FromProto(LogMessage logMessage)
        {
            return new GeTLogMessage(GeTLogMessage.GetLogMessageType(logMessage.Level), logMessage.Message);
        }
    }
}
