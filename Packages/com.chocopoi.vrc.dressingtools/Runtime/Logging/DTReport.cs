/*
 * File: DTReport.cs
 * Project: DressingTools
 * Created Date: Thursday, August 10th 2023, 11:42:41 pm
 * Author: chocopoi (poi@chocopoi.com)
 * -----
 * Copyright (c) 2023 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.Logging
{
    [Serializable]
    public enum DTReportLogType
    {
        Error = -1,
        Info = 0,
        Warning = 1,
        Debug = 2,
        Trace = 3
    }

    [Serializable]
    public class DTReportLogEntry
    {
        public DTReportLogType type;
        public string label;
        public string message;
        public string code;
    }

    [Serializable]
    public class DTReport
    {
        private static readonly Localization.I18n t = Localization.I18n.GetInstance();

        public List<DTReportLogEntry> LogEntries { get; private set; }

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

        public bool HasLogCode(string code)
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

        public bool HasLogCodeByType(DTReportLogType type, string code)
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

        public bool HasLogType(DTReportLogType type)
        {
            foreach (var entry in LogEntries)
            {
                if (entry.type == type)
                {
                    return true;
                }
            }
            return false;
        }

        public void Log(DTReportLogType type, string label, string message, string code = null)
        {
            // TODO: do not output debug, trace unless specified in settings
            if (code != null)
            {
                Debug.Log(string.Format("[DressingTools] [{0}] [{1}] ({2}) {3}", label, type, code, message));
            }
            else
            {
                Debug.Log(string.Format("[DressingTools] [{0}] [{1}] {2}", label, type, message));
            }
            LogEntries.Add(new DTReportLogEntry() { type = type, label = label, code = code, message = message });
        }

        public void LogLocalized(DTReportLogType type, string label, string code, params object[] args)
        {
            Log(type, label, t._(code, args), code);
        }

        public void AppendReport(DTReport report)
        {
            LogEntries.AddRange(new List<DTReportLogEntry>(report.LogEntries));
        }

        public void LogException(string label, Exception exception, string extraMessage = null, string extraCode = null)
        {
            LogError(label, extraMessage != null ? string.Format("{0}: {1}", extraMessage, exception.ToString()) : exception.ToString(), extraCode);
        }

        public void LogExceptionLocalized(string label, Exception exception, string extraCode = null, params object[] args)
        {
            LogException(label, exception, t._(extraCode, args), extraCode);
        }

        public void LogError(string label, string message, string code = null)
        {
            Log(DTReportLogType.Error, label, message, code);
        }

        public void LogErrorLocalized(string label, string code, params object[] args)
        {
            LogLocalized(DTReportLogType.Error, label, code, args);
        }

        public void LogInfo(string label, string message, string code = null)
        {
            Log(DTReportLogType.Info, label, message, code);
        }

        public void LogInfoLocalized(string label, string code, params object[] args)
        {
            LogLocalized(DTReportLogType.Info, label, code, args);
        }

        public void LogWarn(string label, string message, string code = null)
        {
            Log(DTReportLogType.Warning, label, message, code);
        }

        public void LogWarnLocalized(string label, string code, params object[] args)
        {
            LogLocalized(DTReportLogType.Warning, label, code, args);
        }

        public void LogDebug(string label, string message, string code = null)
        {
            Log(DTReportLogType.Debug, label, message, code);
        }

        public void LogDebugLocalized(string label, string code, params object[] args)
        {
            LogLocalized(DTReportLogType.Debug, label, code, args);
        }

        public void LogTrace(string label, string message, string code = null)
        {
            Log(DTReportLogType.Trace, label, message, code);
        }

        public void LogTraceLocalized(string label, string code, params object[] args)
        {
            LogLocalized(DTReportLogType.Trace, label, code, args);
        }
    }
}
