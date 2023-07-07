using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views
{
    internal class WearableConfigViewContainer
    {
        public GameObject targetAvatar;
        public GameObject targetWearable;
        public DTWearableConfig config;
    }

    internal class WearableConfigView : IWearableConfigView
    {
        private static readonly I18n t = I18n.GetInstance();

        private static readonly Dictionary<string, IDTDresser> dressers = new Dictionary<string, IDTDresser>()
        {
            { "Default", new DTDefaultDresser() }
        };

        private WearableConfigPresenter wearableConfigPresenter;

        private WearableConfigViewContainer container;

        private int selectedWearableType;

        private int selectedDresserIndex;

        private DTDresserSettings dresserSettings = null;

        private DTReport dresserReport = null;

        private List<DTBoneMapping> dresserBoneMappings;

        private List<DTObjectMapping> dresserObjectMappings;

        private DTMappingEditorContainer boneMappingEditorContainer = null;

        public WearableConfigView(WearableConfigViewContainer container)
        {
            wearableConfigPresenter = new WearableConfigPresenter(this);
            this.container = container;
        }

        private DTDefaultDresserDynamicsOption ConvertIntToDynamicsOption(int dynamicsOption)
        {
            switch (dynamicsOption)
            {
                case 1:
                    return DTDefaultDresserDynamicsOption.KeepDynamicsAndUseParentConstraintIfNecessary;
                case 2:
                    return DTDefaultDresserDynamicsOption.IgnoreTransform;
                case 3:
                    return DTDefaultDresserDynamicsOption.CopyDynamics;
                case 4:
                    return DTDefaultDresserDynamicsOption.IgnoreAll;
                default:
                case 0:
                    return DTDefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint;
            }
        }

        private void InitializeDTBoneMappingEditorSettings()
        {
            boneMappingEditorContainer = new DTMappingEditorContainer()
            {
                dresserSettings = dresserSettings,
                boneMappings = dresserBoneMappings,
                objectMappings = dresserObjectMappings,
                boneMappingMode = DTWearableMappingMode.Auto,
                objectMappingMode = DTWearableMappingMode.Auto
            };
        }

        private bool foldoutMetaInfo = false;

        private void DrawMetaInfoGUI()
        {
            foldoutMetaInfo = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutMetaInfo, "Meta Information");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (foldoutMetaInfo)
            {
                EditorGUILayout.TextField("Name", "");
                EditorGUILayout.TextField("Author", "");
                GUILayout.Label("Description");
                EditorGUILayout.TextArea("");
            }
        }

        private void DrawTypeGenericGUI()
        {
            // TODO: object mapping
        }

        private bool foldoutDresserReportLogEntries = false;

        private void DrawDresserReportGUI()
        {
            if (dresserReport != null)
            {
                //Result

                switch (dresserReport.Result)
                {
                    case DTReportResult.InvalidSettings:
                        EditorGUILayout.HelpBox(t._("helpbox_error_check_result_invalid_settings"), MessageType.Error);
                        break;
                    case DTReportResult.Incompatible:
                        EditorGUILayout.HelpBox(t._("helpbox_error_check_result_incompatible"), MessageType.Error);
                        break;
                    case DTReportResult.Ok:
                        EditorGUILayout.HelpBox(t._("helpbox_info_check_result_ok"), MessageType.Info);
                        break;
                    case DTReportResult.Compatible:
                        EditorGUILayout.HelpBox(t._("helpbox_warn_check_result_compatible"), MessageType.Warning);
                        break;
                }

                EditorGUILayout.Separator();

                var logEntries = dresserReport.GetLogEntriesAsDictionary();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Errors: " + (logEntries.ContainsKey(DTReportLogType.Error) ? logEntries[DTReportLogType.Error].Count : 0));
                GUILayout.Label("Warnings: " + (logEntries.ContainsKey(DTReportLogType.Warning) ? logEntries[DTReportLogType.Warning].Count : 0));
                GUILayout.Label("Infos: " + (logEntries.ContainsKey(DTReportLogType.Info) ? logEntries[DTReportLogType.Info].Count : 0));
                EditorGUILayout.EndHorizontal();

                foldoutDresserReportLogEntries = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutDresserReportLogEntries, "Logs");
                EditorGUILayout.EndFoldoutHeaderGroup();
                if (foldoutDresserReportLogEntries)
                {
                    if (logEntries.ContainsKey(DTReportLogType.Error))
                    {
                        foreach (var logEntry in logEntries[DTReportLogType.Error])
                        {
                            EditorGUILayout.HelpBox(string.Format("({0}) {1}", logEntry.code.ToString("X4"), logEntry.message), MessageType.Error);
                        }
                    }

                    if (logEntries.ContainsKey(DTReportLogType.Warning))
                    {
                        foreach (var logEntry in logEntries[DTReportLogType.Warning])
                        {
                            EditorGUILayout.HelpBox(string.Format("({0}) {1}", logEntry.code.ToString("X4"), logEntry.message), MessageType.Warning);
                        }
                    }

                    if (logEntries.ContainsKey(DTReportLogType.Info))
                    {
                        foreach (var logEntry in logEntries[DTReportLogType.Info])
                        {
                            EditorGUILayout.HelpBox(string.Format("({0}) {1}", logEntry.code.ToString("X4"), logEntry.message), MessageType.Info);
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox(t._("helpbox_warn_no_check_report"), MessageType.Warning);
            }
        }

        private bool foldoutMapping = true;

        private void DrawTypeArmatureMappingFoldout()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foldoutMapping = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutMapping, "Armature/Root Objects Mapping");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (foldoutMapping)
            {// list all available dressers
                string[] dresserKeys = new string[dressers.Keys.Count];
                dressers.Keys.CopyTo(dresserKeys, 0);
                selectedDresserIndex = EditorGUILayout.Popup("Dressers", selectedDresserIndex, dresserKeys);
                var dresser = dressers[dresserKeys[selectedDresserIndex]];

                // list dresser settings
                if (dresser is DTDefaultDresser)
                {
                    if (!(dresserSettings is DTDefaultDresserSettings))
                    {
                        dresserSettings = new DTDefaultDresserSettings
                        {
                            // TODO: constant defaults?
                            avatarArmatureName = "Armature",
                            wearableArmatureName = "Armature",
                            dynamicsOption = DTDefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint
                        };
                    }

                    var defaultDresserSettings = (DTDefaultDresserSettings)dresserSettings;

                    defaultDresserSettings.targetAvatar = container.targetAvatar;
                    defaultDresserSettings.targetWearable = container.targetWearable;
                    defaultDresserSettings.avatarArmatureName = EditorGUILayout.TextField("Avatar Armature Name", dresserSettings.avatarArmatureName);
                    defaultDresserSettings.wearableArmatureName = EditorGUILayout.TextField("Wearable Armature Name", dresserSettings.wearableArmatureName);

                    // Dynamics Option
                    defaultDresserSettings.dynamicsOption = ConvertIntToDynamicsOption(EditorGUILayout.Popup("Dynamics Option", (int)defaultDresserSettings.dynamicsOption, new string[] {
                        "Remove wearable dynamics and ParentConstraint",
                        "Keep wearable dynamics and ParentConstraint if needed",
                        "Remove wearable dynamics and IgnoreTransform",
                        "Copy avatar dynamics data to wearable",
                        "Ignore all dynamics"
                    }));
                }

                EditorGUILayout.Separator();

                // generate bone mappings
                var mappingBtnStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 16
                };

                if (GUILayout.Button("Auto-generate Mappings", mappingBtnStyle, GUILayout.Height(40)))
                {
                    dresserReport = dresser.Execute(dresserSettings, out dresserBoneMappings, out dresserObjectMappings);
                    InitializeDTBoneMappingEditorSettings();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(dresserBoneMappings == null || dresserObjectMappings == null);
                if (GUILayout.Button("View/Edit Mappings", mappingBtnStyle, GUILayout.Height(40)))
                {
                    var boneMappingEditorWindow = (DTMappingEditorWindow)EditorWindow.GetWindow(typeof(DTMappingEditorWindow));
                    if (boneMappingEditorContainer == null)
                    {
                        InitializeDTBoneMappingEditorSettings();
                    }
                    boneMappingEditorWindow.SetSettings(boneMappingEditorContainer);
                    boneMappingEditorWindow.titleContent = new GUIContent("DT Mapping Editor");
                    boneMappingEditorWindow.Show();
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(dresserReport == null);
                if (GUILayout.Button("View Report", mappingBtnStyle, GUILayout.Height(40)))
                {
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(true);
                if (GUILayout.Button("Test Now", mappingBtnStyle, GUILayout.Height(40)))
                {
                }
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();

                DTUtils.DrawHorizontalLine();

                DrawDresserReportGUI();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawTypeArmatureGUI()
        {
            DrawTypeArmatureMappingFoldout();
        }

        public void OnGUI()
        {
            selectedWearableType = EditorGUILayout.Popup("Wearable Type", selectedWearableType, new string[] { "Generic", "Armature-based" });

            if (selectedWearableType == 0) // Generic
            {
                DrawTypeGenericGUI();
            }
            else if (selectedWearableType == 1) // Armature-based
            {
                DrawTypeArmatureGUI();
            }
        }
    }
}
