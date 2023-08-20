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
using Chocopoi.DressingTools.Lib.Logging;

namespace Chocopoi.DressingTools.Logging
{
    [Serializable]
    public static class DTReportUtils
    {
        private static readonly Localization.I18n t = Localization.I18n.GetInstance();

        public static void LogLocalized(DTReport report, DTReportLogType type, string label, string code, params object[] args)
        {
            report.Log(type, label, t._(code, args), code);
        }

        public static void LogExceptionLocalized(DTReport report, string label, Exception exception, string extraCode = null, params object[] args)
        {
            report.LogException(label, exception, extraCode != null ? t._(extraCode, args) : null, extraCode);
        }

        public static void LogErrorLocalized(DTReport report, string label, string code, params object[] args)
        {
            LogLocalized(report, DTReportLogType.Error, label, code, args);
        }

        public static void LogInfoLocalized(DTReport report, string label, string code, params object[] args)
        {
            LogLocalized(report, DTReportLogType.Info, label, code, args);
        }

        public static void LogWarnLocalized(DTReport report, string label, string code, params object[] args)
        {
            LogLocalized(report, DTReportLogType.Warning, label, code, args);
        }

        public static void LogDebugLocalized(DTReport report, string label, string code, params object[] args)
        {
            LogLocalized(report, DTReportLogType.Debug, label, code, args);
        }

        public static void LogTraceLocalized(DTReport report, string label, string code, params object[] args)
        {
            LogLocalized(report, DTReportLogType.Trace, label, code, args);
        }
    }
}
