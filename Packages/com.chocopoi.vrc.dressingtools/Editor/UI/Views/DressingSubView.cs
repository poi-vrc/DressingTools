using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views
{
    internal class DressingSubView : IDressingSubView
    {
        private static readonly I18n t = I18n.GetInstance();
        private static readonly Dictionary<string, IDTDresser> dressers = new Dictionary<string, IDTDresser>()
        {
            { "Default", new DTDefaultDresser() }
        };

        private DressingPresenter dressingPresenter;

        private GameObject targetAvatar;

        private GameObject targetWearable;

        private int selectedWearableType;

        private int selectedDresserIndex;

        private DTDresserSettings dresserSettings = null;

        private DTReport dresserReport = null;

        private List<DTBoneMapping> dresserBoneMappings;

        private List<DTObjectMapping> dresserObjectMappings;

        private DTMappingEditorSettings boneMappingEditorSettings = null;

        public DressingSubView(IMainView mainView, IMainPresenter mainPresenter)
        {
            dressingPresenter = new DressingPresenter(this);
        }

        private int ConvertDynamicsOptionToInt(DTDefaultDresserDynamicsOption dynamicsOption)
        {
            switch (dynamicsOption)
            {
                case DTDefaultDresserDynamicsOption.KeepDynamicsAndUseParentConstraintIfNecessary:
                    return 1;
                case DTDefaultDresserDynamicsOption.IgnoreTransform:
                    return 2;
                case DTDefaultDresserDynamicsOption.CopyDynamics:
                    return 3;
                case DTDefaultDresserDynamicsOption.IgnoreAll:
                    return 4;
                default:
                case DTDefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint:
                    return 0;
            }
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
            boneMappingEditorSettings = new DTMappingEditorSettings()
            {
                dresserSettings = dresserSettings,
                boneMappings = dresserBoneMappings,
                objectMappings = dresserObjectMappings,
                boneMappingMode = DTWearableMappingMode.Auto,
                objectMappingMode = DTWearableMappingMode.Auto
            };
        }

        public void OnGUI()
        {
            // TODO: beautify UI
            targetAvatar = (GameObject)EditorGUILayout.ObjectField("Avatar", targetAvatar, typeof(GameObject), true);

            var cabinet = DTUtils.GetAvatarCabinet(targetAvatar);
            if (cabinet == null)
            {
                EditorGUILayout.HelpBox("The selected avatar has no existing cabinet.", MessageType.Error);
            }

            targetWearable = (GameObject)EditorGUILayout.ObjectField("Wearable", targetWearable, typeof(GameObject), true);

            selectedWearableType = EditorGUILayout.Popup("Wearable Type", selectedWearableType, new string[] { "Generic", "Armature-based" });

            if (selectedWearableType == 0) // Generic
            {
                // TODO: object mapping
            }
            else if (selectedWearableType == 1) // Armature-based
            {
                // list all available dressers
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

                    defaultDresserSettings.targetAvatar = targetAvatar;
                    defaultDresserSettings.targetWearable = targetWearable;
                    defaultDresserSettings.avatarArmatureName = EditorGUILayout.TextField("Avatar Armature Name", dresserSettings.avatarArmatureName);
                    defaultDresserSettings.wearableArmatureName = EditorGUILayout.TextField("Wearable Armature Name", dresserSettings.wearableArmatureName);

                    // Dynamics Option
                    var radioStyle = new GUIStyle(EditorStyles.radioButton)
                    {
                        wordWrap = true
                    };

                    int dynamicsOption = ConvertDynamicsOptionToInt(defaultDresserSettings.dynamicsOption);
                    dynamicsOption = GUILayout.SelectionGrid(dynamicsOption, new string[] {
                        " " + t._("radio_db_remove_and_parent_const"),
                        " " + t._("radio_db_keep_clothes_and_parent_const_if_need"),
                        " " + t._("radio_db_create_child_and_exclude"),
                        " " + t._("radio_db_copy_dyn_bone_to_clothes"),
                        " " + t._("radio_db_ignore_all")
                    }, 1, radioStyle, GUILayout.ExpandHeight(true), GUILayout.MaxHeight(200));
                    defaultDresserSettings.dynamicsOption = ConvertIntToDynamicsOption(dynamicsOption);
                }

                // generate bone mappings
                if (GUILayout.Button("Generate Bone Mappings"))
                {
                    dresserReport = dresser.Execute(dresserSettings, out dresserBoneMappings, out dresserObjectMappings);
                    InitializeDTBoneMappingEditorSettings();
                }

                if (dresserReport != null)
                {
                    var logEntries = dresserReport.GetLogEntriesAsDictionary();
                    GUILayout.Label("Errors: " + (logEntries.ContainsKey(DTReportLogType.Error) ? logEntries[DTReportLogType.Error].Count : 0));
                    GUILayout.Label("Warnings: " + (logEntries.ContainsKey(DTReportLogType.Warning) ? logEntries[DTReportLogType.Warning].Count : 0));
                    GUILayout.Label("Infos: " + (logEntries.ContainsKey(DTReportLogType.Info) ? logEntries[DTReportLogType.Info].Count : 0));

                    if (logEntries.ContainsKey(DTReportLogType.Error))
                    {
                        foreach (var logEntry in logEntries[DTReportLogType.Error])
                        {
                            EditorGUILayout.HelpBox(string.Format("({0}) {1}", logEntry.code, logEntry.message), MessageType.Error);
                        }
                    }

                    if (logEntries.ContainsKey(DTReportLogType.Warning))
                    {
                        foreach (var logEntry in logEntries[DTReportLogType.Warning])
                        {
                            EditorGUILayout.HelpBox(string.Format("({0}) {1}", logEntry.code, logEntry.message), MessageType.Warning);
                        }
                    }

                    if (logEntries.ContainsKey(DTReportLogType.Info))
                    {
                        foreach (var logEntry in logEntries[DTReportLogType.Info])
                        {
                            EditorGUILayout.HelpBox(string.Format("({0}) {1}", logEntry.code, logEntry.message), MessageType.Info);
                        }
                    }

                    if (GUILayout.Button("View Logs"))
                    {
                        // TODO: Report window
                    }
                }

                if (dresserBoneMappings != null)
                {
                    if (GUILayout.Button("View and Edit Bone Mappings"))
                    {
                        var boneMappingEditorWindow = (DTMappingEditorWindow)EditorWindow.GetWindow(typeof(DTMappingEditorWindow));
                        if (boneMappingEditorSettings == null)
                        {
                            InitializeDTBoneMappingEditorSettings();
                        }
                        boneMappingEditorWindow.SetSettings(boneMappingEditorSettings);
                        boneMappingEditorWindow.titleContent = new GUIContent("DT Mapping Editor");
                        boneMappingEditorWindow.Show();
                    }
                }
            }
        }
    }
}
