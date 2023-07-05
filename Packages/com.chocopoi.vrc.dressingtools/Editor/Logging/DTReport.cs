using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.Logging
{
    [Serializable]
    public enum DTReportLogType
    {
        Fatal = -2,
        Error = -1,
        Info = 0,
        Warning = 1,
        Debug = 2,
        Trace = 3
    }

    [Serializable]
    public enum DTReportResult
    {
        InvalidSettings = -2,
        Incompatible = -1,
        Ok = 0,
        Compatible = 1
    }

    [Serializable]
    public class DTReportLogEntry
    {
        public DTReportLogType type;
        public int code;
        public string message;
    }

    [Serializable]
    public class DTReport
    {
        public List<DTReportLogEntry> LogEntries { get; private set; }

        public DTReportResult Result { get; set; }

        public DTReport()
        {
            LogEntries = new List<DTReportLogEntry>();
        }

        public Dictionary<DTReportLogType, List<DTReportLogEntry>> GetLogEntriesAsDictionary()
        {
            var dict = new Dictionary<DTReportLogType, List<DTReportLogEntry>>();
            foreach (var entry in LogEntries)
            {
                if (!dict.ContainsKey(entry.type))
                {
                    dict.Add(entry.type, new List<DTReportLogEntry>());
                }
                dict[entry.type].Add(entry);
            }
            return dict;
        }

        public bool HasLogCode(int code)
        {
            foreach (var entry in LogEntries)
            {
                if (entry.code == code)
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasLogCodeByType(DTReportLogType type, int code)
        {
            foreach (var entry in LogEntries)
            {
                if (entry.type == type && entry.code == code)
                {
                    return true;
                }
            }
            return false;
        }

        public void Log(DTReportLogType type, int code, string message)
        {
            Debug.Log(string.Format("[{0}] ({1}) {2}", type, code.ToString("X4"), message));
            LogEntries.Add(new DTReportLogEntry() { type = type, code = code, message = message });
        }

        public void LogError(int code, string message)
        {
            Log(DTReportLogType.Error, code, message);
        }

        public void LogInfo(int code, string message)
        {
            Log(DTReportLogType.Info, code, message);
        }

        public void LogWarn(int code, string message)
        {
            Log(DTReportLogType.Warning, code, message);
        }

        public void LogDebug(int code, string message)
        {
            Log(DTReportLogType.Debug, code, message);
        }
    }
}
