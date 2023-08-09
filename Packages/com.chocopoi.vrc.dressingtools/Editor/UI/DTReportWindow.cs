using System.Collections.Generic;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.Logging;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI
{
    public class DTReportWindow : EditorWindow
    {
        private static readonly I18n t = I18n.GetInstance();

        private DTReport _report;

        private Dictionary<DTReportLogType, List<DTReportLogEntry>> _logEntries;

        private Vector2 scrollPos;

        public static void ShowWindow(DTReport report)
        {
            var window = (DTReportWindow)GetWindow(typeof(DTReportWindow));
            window.titleContent = new GUIContent("DT Report Window");
            window.Show();

            window._report = report;
            window._logEntries = report.GetLogEntriesAsDictionary();
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
                //Result

                if (_report.HasLogType(DTReportLogType.Error))
                {
                    EditorGUILayout.HelpBox(t._("helpbox_error_check_result_incompatible"), MessageType.Error);
                }
                else if (_report.HasLogType(DTReportLogType.Warning))
                {
                    EditorGUILayout.HelpBox(t._("helpbox_warn_check_result_compatible"), MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox(t._("helpbox_info_check_result_ok"), MessageType.Info);
                }

                EditorGUILayout.Separator();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Errors: " + (_logEntries.ContainsKey(DTReportLogType.Error) ? _logEntries[DTReportLogType.Error].Count : 0));
                    GUILayout.Label("Warnings: " + (_logEntries.ContainsKey(DTReportLogType.Warning) ? _logEntries[DTReportLogType.Warning].Count : 0));
                    GUILayout.Label("Infos: " + (_logEntries.ContainsKey(DTReportLogType.Info) ? _logEntries[DTReportLogType.Info].Count : 0));
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
                EditorGUILayout.HelpBox(t._("helpbox_warn_no_check_report"), MessageType.Warning);
            }
        }
    }
}
