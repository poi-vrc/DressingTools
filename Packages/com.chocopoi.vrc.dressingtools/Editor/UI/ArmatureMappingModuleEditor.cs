using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Modules
{
    [CustomModuleEditor(typeof(ArmatureMappingModule))]
    public class ArmatureMappingModuleEditor : ModuleEditor
    {
        private static Localization.I18n t = Localization.I18n.GetInstance();

        // used to detect changes and regenerate mappings
        private GameObject lastTargetAvatar;

        private GameObject lastTargetWearable;

        private string selectedDresserName = "Default";

        private DTDresserSettings dresserSettings = null;

        private DTReport dresserReport = null;

        private bool regenerateMappingsNeeded = false;

        private bool foldoutDresserReportLogEntries = false;

        private DTMappingEditorContainer mappingEditorContainer;

        private DTCabinet cabinet;

        public ArmatureMappingModuleEditor(DTWearableModuleBase target, DTWearableConfig config) : base(target, config)
        {
            mappingEditorContainer = new DTMappingEditorContainer();
            ResetMappingEditorContainer();
        }

        private void ResetMappingEditorContainer()
        {
            mappingEditorContainer.dresserSettings = null;
            mappingEditorContainer.boneMappings = null;
            mappingEditorContainer.boneMappingMode = DTBoneMappingMode.Auto;
        }

        public DTReport GenerateDresserMappings(IDTDresser dresser, DTDresserSettings dresserSettings)
        {
            // reset mapping editor container
            ResetMappingEditorContainer();
            mappingEditorContainer.dresserSettings = dresserSettings;

            // execute dresser
            var dresserReport = dresser.Execute(dresserSettings, out mappingEditorContainer.boneMappings);

            return dresserReport;
        }

        public void StartMappingEditor()
        {
            var boneMappingEditorWindow = (DTMappingEditorWindow)EditorWindow.GetWindow(typeof(DTMappingEditorWindow));

            boneMappingEditorWindow.SetSettings(mappingEditorContainer);
            boneMappingEditorWindow.titleContent = new GUIContent("DT Mapping Editor");
            boneMappingEditorWindow.Show();
        }

        private void InitializeDresserSettings(ArmatureMappingModule module)
        {
            var dresser = DresserRegistry.GetDresserByName(selectedDresserName);
            dresserSettings = dresser.DeserializeSettings(module.serializedDresserConfig ?? "{}");
            if (dresserSettings == null)
            {
                dresserSettings = dresser.NewSettings();
            }
        }

        private void DrawDresserReportGUI()
        {
            if (dresserReport != null)
            {
                //Result

                if (dresserReport.HasLogType(DTReportLogType.Error))
                {
                    EditorGUILayout.HelpBox(t._("helpbox_error_check_result_incompatible"), MessageType.Error);
                }
                else if (dresserReport.HasLogType(DTReportLogType.Warning))
                {
                    EditorGUILayout.HelpBox(t._("helpbox_warn_check_result_compatible"), MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox(t._("helpbox_info_check_result_ok"), MessageType.Info);
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
                            EditorGUILayout.HelpBox(logEntry.message, MessageType.Error);
                        }
                    }

                    if (logEntries.ContainsKey(DTReportLogType.Warning))
                    {
                        foreach (var logEntry in logEntries[DTReportLogType.Warning])
                        {
                            EditorGUILayout.HelpBox(logEntry.message, MessageType.Warning);
                        }
                    }

                    if (logEntries.ContainsKey(DTReportLogType.Info))
                    {
                        foreach (var logEntry in logEntries[DTReportLogType.Info])
                        {
                            EditorGUILayout.HelpBox(logEntry.message, MessageType.Info);
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox(t._("helpbox_warn_no_check_report"), MessageType.Warning);
            }
        }

        public override bool OnGUI(GameObject avatarGameObject, GameObject wearableGameObject)
        {
            var module = (ArmatureMappingModule)target;

            // initial dresser settings if null
            if (dresserSettings == null)
            {
                InitializeDresserSettings(module);
            }

            // list all available dressers
            string[] dresserKeys = DresserRegistry.GetAvailableDresserKeys();
            var selectedDresserIndex = EditorGUILayout.Popup("Dressers", System.Array.IndexOf(dresserKeys, selectedDresserName), dresserKeys);

            if (dresserKeys[selectedDresserIndex] != selectedDresserName)
            {
                // regenerate on dresser change
                regenerateMappingsNeeded = true;
            }
            selectedDresserName = dresserKeys[selectedDresserIndex];

            var dresser = DresserRegistry.GetDresserByName(selectedDresserName);

            // set the type name to config
            module.dresserName = dresser.GetType().FullName;

            // reinitialize dresser settings if not correct type
            if (dresser is DTDefaultDresser && !(dresserSettings is DTDefaultDresserSettings))
            {
                InitializeDresserSettings(module);
            }

            if (lastTargetAvatar != avatarGameObject || lastTargetWearable != wearableGameObject)
            {
                regenerateMappingsNeeded = true;
                cabinet = DTEditorUtils.GetAvatarCabinet(avatarGameObject);
                lastTargetAvatar = avatarGameObject;
                lastTargetWearable = wearableGameObject;
            }

            // draw the dresser settings GUI and regenerate if modified
            dresserSettings.targetAvatar = avatarGameObject;
            dresserSettings.targetWearable = wearableGameObject;
            if (cabinet != null)
            {
                EditorGUILayout.HelpBox("The avatar is associated with a cabinet. To change the avatar Armature name, please use the cabinet editor.", MessageType.Info);
                dresserSettings.avatarArmatureName = cabinet.avatarArmatureName;
            }
            EditorGUI.BeginDisabledGroup(cabinet != null);
            var newAvatarArmatureName = EditorGUILayout.DelayedTextField("Avatar Armature Name", dresserSettings.avatarArmatureName);
            regenerateMappingsNeeded |= newAvatarArmatureName != dresserSettings.avatarArmatureName;
            dresserSettings.avatarArmatureName = newAvatarArmatureName;
            EditorGUI.EndDisabledGroup();

            if (dresserSettings.DrawEditorGUI())
            {
                regenerateMappingsNeeded = true;

                // serialize if modified
                module.serializedDresserConfig = JsonConvert.SerializeObject(dresserSettings);
            }

            EditorGUILayout.Separator();

            // generate bone mappings

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Regenerate Mappings"))
            {
                regenerateMappingsNeeded = true;
            }
            if (GUILayout.Button("View/Edit Mappings"))
            {
                StartMappingEditor();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(dresserReport == null);
            if (GUILayout.Button("View Report"))
            {
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(true);
            if (GUILayout.Button("Test Now"))
            {
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            DTEditorUtils.DrawHorizontalLine();

            DrawDresserReportGUI();

            if (regenerateMappingsNeeded)
            {
                regenerateMappingsNeeded = false;
                dresserReport = GenerateDresserMappings(dresser, dresserSettings);
            }

            return false;
        }

        public override bool IsValid()
        {
            var module = (ArmatureMappingModule)target;

            // copy wearable armature name from dresser settings and serialize dresser settings
            if (dresserSettings != null)
            {
                module.wearableArmatureName = dresserSettings.wearableArmatureName;
            }
            module.serializedDresserConfig = JsonConvert.SerializeObject(dresserSettings);

            // update values from mapping editor container
            module.boneMappingMode = mappingEditorContainer.boneMappingMode;
            module.boneMappings = module.boneMappingMode != DTBoneMappingMode.Auto ? mappingEditorContainer.boneMappings?.ToArray() : new DTBoneMapping[0];

            return dresserReport != null && !dresserReport.HasLogType(DTReportLogType.Error) && module.boneMappings != null;
        }
    }
}
