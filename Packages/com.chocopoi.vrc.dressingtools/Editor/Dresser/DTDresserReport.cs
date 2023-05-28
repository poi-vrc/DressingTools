using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.Dresser
{
    [Serializable]
    public enum DTDresserReportLogType
    {
        Fatal = -2,
        Error = -1,
        Info = 0,
        Warning = 1,
        Debug = 2,
        Trace = 3
    }

    [Serializable]
    public enum DTDresserReportResult
    {
        INVALID_SETTINGS = -2,
        INCOMPATIBLE = -1,
        OK = 0,
        COMPATIBLE = 1
    }

    [Serializable]
    public class DTDresserReportLogEntry
    {
        public DTDresserReportLogType type;
        public int code;
        public string message;
    }

    [Serializable]
    public class DTDresserReport
    {
        private List<DTDresserReportLogEntry> logEntries;

        public DTDresserReport()
        {
            logEntries = new List<DTDresserReportLogEntry>();
        }

        public void Log(DTDresserReportLogType type, int code, string message)
        {
            logEntries.Add(new DTDresserReportLogEntry() { type = type, code = code, message = message });
        }

        public void LogError(int code, string message)
        {
            Log(DTDresserReportLogType.Error, code, message);
        }

        public void LogInfo(int code, string message)
        {
            Log(DTDresserReportLogType.Info, code, message);
        }

        public void LogWarn(int code, string message)
        {
            Log(DTDresserReportLogType.Warning, code, message);
        }

        public void LogDebug(int code, string message)
        {
            Log(DTDresserReportLogType.Debug, code, message);
        }
    }
}
