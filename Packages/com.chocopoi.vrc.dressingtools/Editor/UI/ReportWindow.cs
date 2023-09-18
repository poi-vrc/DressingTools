/*
 * File: ReportWindow.cs
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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingTools.Lib.Logging;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI
{
    [ExcludeFromCodeCoverage]
    internal class ReportWindow : EditorWindow
    {
        private static readonly Localization.I18n t = Localization.I18n.Instance;

        private DTReport _report;

        private Dictionary<DTReportLogType, List<DTReportLogEntry>> _logEntries;

        private Vector2 scrollPos;
        public static void ShowWindow(DTReport report)
        {
            var window = (ReportWindow)GetWindow(typeof(ReportWindow));
            window.titleContent = new GUIContent(t._("report.editor.title"));
            window.Show();

            window._report = report;
        }

        public void ResetReport()
        {
            _report = null;
            _logEntries = null;
        }

        public void OnGUI()
        {
            if (_report != null)
            {
                if (_logEntries == null)
                {
                    _logEntries = _report.GetLogEntriesAsDictionary();
                }
                //Result

                if (_report.HasLogType(DTReportLogType.Error))
                {
                    EditorGUILayout.HelpBox(t._("report.editor.helpbox.resultError"), MessageType.Error);
                }
                else if (_report.HasLogType(DTReportLogType.Warning))
                {
                    EditorGUILayout.HelpBox(t._("report.editor.helpbox.resultWarn"), MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox(t._("report.editor.helpbox.resultSuccess"), MessageType.Info);
                }

                EditorGUILayout.Separator();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label(t._("report.editor.label.errors", _logEntries.ContainsKey(DTReportLogType.Error) ? _logEntries[DTReportLogType.Error].Count : 0));
                    GUILayout.Label(t._("report.editor.label.warnings", _logEntries.ContainsKey(DTReportLogType.Warning) ? _logEntries[DTReportLogType.Warning].Count : 0));
                    GUILayout.Label(t._("report.editor.label.infos", _logEntries.ContainsKey(DTReportLogType.Info) ? _logEntries[DTReportLogType.Info].Count : 0));
                }
                EditorGUILayout.EndHorizontal();

                // show logs
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                {
                    if (_logEntries.ContainsKey(DTReportLogType.Error))
                    {
                        foreach (var logEntry in _logEntries[DTReportLogType.Error])
                        {
                            EditorGUILayout.HelpBox(logEntry.message, MessageType.Error);
                        }
                    }

                    if (_logEntries.ContainsKey(DTReportLogType.Warning))
                    {

                        foreach (var logEntry in _logEntries[DTReportLogType.Warning])
                        {
                            EditorGUILayout.HelpBox(logEntry.message, MessageType.Warning);
                        }
                    }

                    if (_logEntries.ContainsKey(DTReportLogType.Info))
                    {
                        foreach (var logEntry in _logEntries[DTReportLogType.Info])
                        {
                            EditorGUILayout.HelpBox(logEntry.message, MessageType.Info);
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.HelpBox(t._("report.helpbox.noReport"), MessageType.Warning);
            }
        }
    }
}
