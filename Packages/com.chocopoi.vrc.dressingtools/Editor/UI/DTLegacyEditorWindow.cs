/*
 * File: DTLegacyEditorWindow.cs
 * Project: DressingTools
 * Created Date: Saturday, July 29th 2023, 10:31:11 am
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
using System.Text.RegularExpressions;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.Lib.Logging;
using Chocopoi.DressingTools.Localization;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Chocopoi.DressingTools.UI
{
    public class DTLegacyEditorWindow : EditorWindow
    {
        private static I18n t = I18n.GetInstance();

        private static readonly DressingToolsUpdater.ParsedVersion CurrentVersion = DressingToolsUpdater.GetCurrentVersion();

        private static readonly Regex IllegalCharactersRegex = new Regex("[^a-zA-Z0-9_-]");

        private static readonly DefaultDresser DefaultDresser = new DefaultDresser();

        private static AnimatorController s_testModeAnimationController;

        private int _dynamicBoneOption = 0;

        private GameObject _activeAvatar;

        private GameObject _clothesToDress;

        private GameObject _lastClothesToDress;

        private string _newClothesName;

        //private bool useDefaultGeneratedPrefixSuffix = true;

        //private string prefixToBeAdded;

        //private string suffixToBeAdded;

        private bool _useCustomArmatureObjectNames = false;

        private string _avatarArmatureObjectName;

        private string _clothesArmatureObjectName;

        private bool _removeExistingPrefixSuffix = true;

        private bool _groupBones = true;

        private bool _groupDynamics = true;

        private bool _dressNowConfirm = false;

        private int _selectedInterface = 0;

        private DTReport _report = null;

        //private bool showStatisticsFoldout = false;

        private Vector2 _scrollPos;

        private bool _needClearDirty = false;

        //private bool updateAvailableFoldout = false;

        [MenuItem("Tools/chocopoi/Legacy v1 Editor", false, 0)]
        public static void Init()
        {
            var window = (DTLegacyEditorWindow)GetWindow(typeof(DTLegacyEditorWindow));
            window.titleContent = new GUIContent("DressingTools (v1)");
            window.Show();
        }

        public void OnEnable()
        {
            s_testModeAnimationController = AssetDatabase.LoadAssetAtPath<AnimatorController>("Packages/com.chocopoi.vrc.dressingtools/Animations/TestModeAnimationController.controller");

            if (s_testModeAnimationController == null)
            {
                Debug.LogError("[DressingTools] Could not load \"TestModeAnimationController\" from \"Assets/chocopoi/DressingTools/Animations\". Did you move it to another location?");
            }
        }

        private void DrawToolHeaderGUI()
        {
            DTLogo.Show();

            EditorGUILayout.Separator();

            //if (DressingToolsUpdater.IsUpdateChecked() && !DressingToolsUpdater.IsLastUpdateCheckErrored() && DressingToolsUpdater.IsUpdateAvailable())
            //{
            //    var branch = DressingToolsUpdater.GetBranchLatestVersion(Preferences.GetPreferences().app.update_branch);

            //    var updateFoldoutStyle = new GUIStyle(EditorStyles.foldout);
            //    updateFoldoutStyle.normal.textColor = Color.red;
            //    updateFoldoutStyle.onNormal.textColor = Color.red;
            //    updateFoldoutStyle.fontStyle = FontStyle.Bold;

            //    if (updateAvailableFoldout = EditorGUILayout.Foldout(updateAvailableFoldout, "<" + t._("foldout_update_available") + "> " + DressingToolsUpdater.GetCurrentVersion().full_version_string + " -> " + branch.version, updateFoldoutStyle))
            //    {
            //        GUILayout.BeginHorizontal();
            //        GUILayout.Space(20);
            //        if (GUILayout.Button("> " + t._("button_download_from_booth"), EditorStyles.label))
            //        {
            //            Application.OpenURL(branch?.booth_url);
            //        }
            //        GUILayout.EndHorizontal();
            //        GUILayout.BeginHorizontal();
            //        GUILayout.Space(20);
            //        if (GUILayout.Button("> " + t._("button_download_from_github"), EditorStyles.label))
            //        {
            //            Application.OpenURL(branch?.github_url);
            //        }
            //        GUILayout.EndHorizontal();
            //    }
            //}

            EditorGUILayout.HelpBox("You are not recommended to use this editor anymore since it will not contain the latest v2 features. It is simply an old layout that assembles back to use v2 scripts.", MessageType.Error);

            DTEditorUtils.DrawHorizontalLine();
        }

        private void Update()
        {
            // a dirty way to run repaint on main thread
            if (_needClearDirty)
            {
                _needClearDirty = false;
                Repaint();
            }
        }

        private void FinishFetchOnlineVersion(DressingToolsUpdater.Manifest manifest)
        {
            //force redraw
            _needClearDirty = true;
        }

        private void DrawToolFooterGUI()
        {
            DTEditorUtils.DrawHorizontalLine();

            GUILayout.Label(t._("label_footer_version", CurrentVersion?.fullVersionString));

            if (DressingToolsUpdater.IsUpdateChecked())
            {
                if (DressingToolsUpdater.IsLastUpdateCheckErrored())
                {
                    GUILayout.Label(t._("label_could_not_check_update"));
                }
                else if (DressingToolsUpdater.IsUpdateAvailable())
                {
                    //var branch = DressingToolsUpdater.GetBranchLatestVersion(Preferences.GetPreferences().app.update_branch);

                    //GUILayout.Label(t._("label_update_available", branch?.version));
                    GUILayout.Label(t._("label_update_available", ""));
                }
                else
                {
                    GUILayout.Label(t._("label_up_to_date"));
                }
            }
            else
            {
                GUILayout.Label(t._("label_checking_for_updates"));
                DressingToolsUpdater.FetchOnlineVersion(FinishFetchOnlineVersion);
            }

            EditorGUILayout.SelectableLabel("https://github.com/poi-vrc/DressingTools");
        }

        private void DrawReportResult()
        {
            if (_report == null)
            {
                EditorGUILayout.HelpBox(t._("helpbox_warn_no_check_report"), MessageType.Warning);
                return;
            }

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
        }

        private void DrawLogEntries(Dictionary<DTReportLogType, List<DTReportLogEntry>> logEntries)
        {
            if (logEntries.Count == 0)
            {
                EditorGUILayout.LabelField(t._("label_no_problems_found"));
                return;
            }

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

        private void DrawDressReportDetails()
        {
            if (_report == null)
            {
                return;
            }

            //showStatisticsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(showStatisticsFoldout, t._("foldout_dressing_statistics"));

            //if (showStatisticsFoldout)
            //{
            //    GUILayout.Label(t._("label_statistics_total_avatar_dynbones", dressReport.avatarDynBones.Count));

            //    GUILayout.Label(t._("label_statistics_total_avatar_physbones", dressReport.avatarPhysBones.Count));

            //    GUILayout.Label(t._("label_statistics_total_clothes_dynbones", dressReport.clothesDynBones.Count, dressReport.clothesOriginalDynBones.Count));

            //    GUILayout.Label(t._("label_statistics_total_clothes_physbones", dressReport.clothesPhysBones.Count, dressReport.clothesOriginalPhysBones.Count));

            //    GUILayout.Label(t._("label_statistics_total_clothes_objects", dressReport.clothesAllObjects.Count));

            //    GUILayout.Label(t._("label_statistics_total_clothes_mesh_data", dressReport.clothesMeshDataObjects.Count));
            //}
            //EditorGUILayout.EndFoldoutHeaderGroup();

            //EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Logs", EditorStyles.boldLabel);

            DrawLogEntries(_report.GetLogEntriesAsDictionary());
        }

        private DefaultDresserSettings MakeDressSettings()
        {
            DefaultDresserDynamicsOption dynamicsOption;
            switch (_dynamicBoneOption)
            {
                default:
                case 0:
                    dynamicsOption = DefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint;
                    break;
                case 1:
                    dynamicsOption = DefaultDresserDynamicsOption.KeepDynamicsAndUseParentConstraintIfNecessary;
                    break;
                case 2:
                    dynamicsOption = DefaultDresserDynamicsOption.IgnoreTransform;
                    break;
                case 3:
                    dynamicsOption = DefaultDresserDynamicsOption.CopyDynamics;
                    break;
                case 4:
                    dynamicsOption = DefaultDresserDynamicsOption.IgnoreAll;
                    break;
            }

            return new DefaultDresserSettings()
            {
                targetAvatar = _activeAvatar,
                targetWearable = _clothesToDress,
                dynamicsOption = dynamicsOption,
                avatarArmatureName = _avatarArmatureObjectName,
                wearableArmatureName = _clothesArmatureObjectName
            };
        }

        private void DrawNewClothesNameGUI()
        {
            if (_clothesToDress != null && (_clothesToDress.name == null || _clothesToDress.name == "" || IllegalCharactersRegex.IsMatch(_clothesToDress.name)))
            {
                //EditorGUILayout.HelpBox(t._("helpbox_error_clothes_name_illegal_characters_detected"), MessageType.Error);
                EditorGUILayout.HelpBox(t._("helpbox_warn_clothes_name_illegal_characters_detected_may_not_compatible_in_future_versions"), MessageType.Warning);
                if (_newClothesName == null || _newClothesName == "")
                {
                    _newClothesName = "NewClothes_" + new System.Random().Next();
                }
                _newClothesName = IllegalCharactersRegex.Replace(_newClothesName, "");
            }

            if (_newClothesName == null || _lastClothesToDress != _clothesToDress)
            {
                _newClothesName = _clothesToDress?.name;
            }
            _lastClothesToDress = _clothesToDress;

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(_clothesToDress == null);
            _newClothesName = EditorGUILayout.TextField(t._("text_new_clothes_name"), _newClothesName);
            if (GUILayout.Button(t._("button_rename_clothes_name"), GUILayout.ExpandWidth(false)))
            {
                _clothesToDress.name = _newClothesName;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCustomArmatureNameGUI()
        {
            _useCustomArmatureObjectNames = GUILayout.Toggle(_useCustomArmatureObjectNames, t._("toggle_use_custom_armature_object_names"));

            if (!_useCustomArmatureObjectNames)
            {
                _avatarArmatureObjectName = "Armature";
                _clothesArmatureObjectName = "Armature";
            }

            EditorGUI.BeginDisabledGroup(!_useCustomArmatureObjectNames);
            EditorGUI.indentLevel = 1;
            _avatarArmatureObjectName = EditorGUILayout.TextField(t._("text_custom_avatar_armature_object_name"), _avatarArmatureObjectName);
            _clothesArmatureObjectName = EditorGUILayout.TextField(t._("text_custom_clothes_armature_object_name"), _clothesArmatureObjectName);
            EditorGUI.indentLevel = 0;
            EditorGUI.EndDisabledGroup();
        }

        private void DrawSimpleGUI()
        {
            GUILayout.Label(t._("label_setup"), EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(t._("helpbox_info_move_clothes_into_place"), MessageType.Info);

#if VRC_SDK_VRCSDK3
            if (_activeAvatar == null)
            {
                VRC.SDKBase.VRC_AvatarDescriptor[] avatars = FindObjectsOfType<VRC.SDKBase.VRC_AvatarDescriptor>();
                foreach (var avatar in avatars)
                {
                    if (!avatar.name.StartsWith("DressingToolsPreview_"))
                    {
                        _activeAvatar = avatar.gameObject;
                    }
                }
            }
#endif
            _activeAvatar = (GameObject)EditorGUILayout.ObjectField(t._("object_active_avatar"), _activeAvatar, typeof(GameObject), true);

            _clothesToDress = (GameObject)EditorGUILayout.ObjectField(t._("object_clothes_to_dress"), _clothesToDress, typeof(GameObject), true);

            DrawNewClothesNameGUI();

            DrawCustomArmatureNameGUI();

            // simple mode defaults to group dynamics

            _groupDynamics = true;

            // simple mode defaults to use generated prefix

            //useDefaultGeneratedPrefixSuffix = true;

            //if (clothesToDress != null)
            //{
            //    prefixToBeAdded = "";
            //    suffixToBeAdded = " (" + clothesToDress.name + ")";
            //}

            EditorGUILayout.Separator();

            if (_clothesToDress != null)
            {
                EditorGUILayout.LabelField(t._("label_new_bone_name_preview", _clothesToDress.name));
            }

            // simple mode defaults to handle dynamic bones automatically

            _dynamicBoneOption = 0;

            EditorGUILayout.LabelField(t._("label_dynamic_bone_auto_handled"));
        }

        private void DrawAdvancedGUI()
        {
            GUILayout.Label(t._("label_select_avatar"), EditorStyles.boldLabel);

#if VRC_SDK_VRCSDK3
            if (_activeAvatar == null)
            {
                VRC.SDKBase.VRC_AvatarDescriptor[] avatars = FindObjectsOfType<VRC.SDKBase.VRC_AvatarDescriptor>();
                foreach (var avatar in avatars)
                {
                    if (!avatar.name.StartsWith("DressingToolsPreview_"))
                    {
                        _activeAvatar = avatar.gameObject;
                    }
                }
            }
#endif
            _activeAvatar = (GameObject)EditorGUILayout.ObjectField(t._("object_active_avatar"), _activeAvatar, typeof(GameObject), true);

            EditorGUILayout.Separator();

            GUILayout.Label(t._("label_select_clothes_to_dress"), EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(t._("helpbox_info_move_clothes_into_place"), MessageType.Info);

            _clothesToDress = (GameObject)EditorGUILayout.ObjectField(t._("object_clothes_to_dress"), _clothesToDress, typeof(GameObject), true);

            DrawNewClothesNameGUI();

            DrawCustomArmatureNameGUI();

            DTEditorUtils.DrawHorizontalLine();

            GUILayout.Label(t._("label_grouping_bones_root_objects_dynamics"), EditorStyles.boldLabel);

            EditorGUILayout.Separator();

            _groupBones = GUILayout.Toggle(_groupBones, t._("toggle_group_bones"));

            _groupDynamics = GUILayout.Toggle(_groupDynamics, t._("toggle_group_dynamics"));

            DTEditorUtils.DrawHorizontalLine();

            GUILayout.Label(t._("label_prefix_suffix"), EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(t._("helpbox_info_prefix_suffix"), MessageType.Info);

            //useDefaultGeneratedPrefixSuffix = GUILayout.Toggle(useDefaultGeneratedPrefixSuffix, t._("toggle_use_default_generated_prefix_suffix"));

            //if (useDefaultGeneratedPrefixSuffix && clothesToDress != null)
            //{
            //    prefixToBeAdded = "";
            //    suffixToBeAdded = " (" + clothesToDress.name + ")";
            //}

            //EditorGUI.BeginDisabledGroup(useDefaultGeneratedPrefixSuffix);
            //EditorGUI.indentLevel = 1;
            //prefixToBeAdded = EditorGUILayout.TextField(t._("text_prefix_to_be_added"), prefixToBeAdded);
            //suffixToBeAdded = EditorGUILayout.TextField(t._("text_suffix_to_be_added"), suffixToBeAdded);
            //EditorGUI.indentLevel = 0;
            //EditorGUI.EndDisabledGroup();

            EditorGUILayout.Separator();

            _removeExistingPrefixSuffix = GUILayout.Toggle(_removeExistingPrefixSuffix, t._("toggle_remove_existing_prefix_suffix_in_clothes_bone"));

            DTEditorUtils.DrawHorizontalLine();

            GUILayout.Label(t._("label_dynamic_bone"), EditorStyles.boldLabel);

            EditorGUILayout.Separator();

            GUILayout.Label(t._("label_dynamic_bone_if_in_avatar_bone"));

            var radioStyle = new GUIStyle(EditorStyles.radioButton)
            {
                wordWrap = true
            };

            _dynamicBoneOption = GUILayout.SelectionGrid(_dynamicBoneOption, new string[] {
                " " + t._("radio_db_remove_and_parent_const"),
                " " + t._("radio_db_keep_clothes_and_parent_const_if_need"),
                " " + t._("radio_db_create_child_and_exclude"),
                " " + t._("radio_db_copy_dyn_bone_to_clothes"),
                " " + t._("radio_db_ignore_all")
            }, 1, radioStyle, GUILayout.ExpandHeight(true), GUILayout.MaxHeight(200));

            GUILayout.Label(t._("helpbox_info_dyn_bone_config_details"));
            EditorGUILayout.SelectableLabel("https://github.com/poi-vrc/DressingTools/wiki/DynamicBone-Configuration");
        }

        private void GenerateMappingsAndApply(bool write)
        {
            // clean up old objects
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj != null && obj.name.StartsWith("DressingToolsPreview_"))
                {
                    DestroyImmediate(obj);
                }
            }

            // replicate the v1 behaviour to generate a preview GameObject
            string avatarNewName = "DressingToolsPreview_" + _activeAvatar.name;

            GameObject targetAvatar;
            GameObject targetWearable;

            if (write)
            {
                // write directly
                targetAvatar = _activeAvatar;
                targetWearable = _clothesToDress;
            }
            else
            {
                // create a copy of the avatar and wearable
                targetAvatar = Instantiate(_activeAvatar);
                targetAvatar.name = avatarNewName;

                var newAvatarPosition = targetAvatar.transform.position;
                newAvatarPosition.x -= 20;
                targetAvatar.transform.position = newAvatarPosition;

                // if clothes is not inside avatar, we instantiate a new copy
                if (!DTRuntimeUtils.IsGrandParent(_activeAvatar.transform, _clothesToDress.transform))
                {
                    targetWearable = Instantiate(_clothesToDress);

                    var newClothesPosition = targetWearable.transform.position;
                    newClothesPosition.x -= 20;
                    targetWearable.transform.position = newClothesPosition;
                }
                else
                {
                    // otherwise, we find the inner wearable and use it
                    targetWearable = targetAvatar.transform.Find(_clothesToDress.name).gameObject;
                }

                var animator = targetAvatar.GetComponent<Animator>();

                //add animation controller
                if (animator != null)
                {
                    animator.runtimeAnimatorController = s_testModeAnimationController;
                }

                //add dummy focus sceneview script
                targetAvatar.AddComponent<DummyFocusSceneViewScript>();
            }

            // check if it's a prefab
            if (PrefabUtility.IsPartOfAnyPrefab(targetWearable))
            {
                if (PrefabUtility.GetPrefabInstanceStatus(_clothesToDress) == PrefabInstanceStatus.NotAPrefab)
                {
                    // if not in scene, we cannot just unpack it but need to instantiate first
                    targetWearable = (GameObject)PrefabUtility.InstantiatePrefab(targetWearable);
                    targetWearable.name = _clothesToDress.name;
                }

                // unpack completely the prefab
                PrefabUtility.UnpackPrefabInstance(targetWearable, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }

            // parent to avatar
            targetWearable.name = _clothesToDress.name;
            targetWearable.transform.SetParent(targetAvatar.transform);

            _report = DefaultDresser.Execute(MakeDressSettings(), out var boneMappings);

            var avatarDynamics = DTRuntimeUtils.ScanDynamics(targetAvatar, true);
            var wearableDynamics = DTRuntimeUtils.ScanDynamics(targetWearable, false);

            throw new System.NotImplementedException("extract v2 ArmatureMappingModule stuff here");

            //if (!DefaultApplier.ApplyBoneMappings(report, applierSettings, clothesToDress.name, avatarDynamics, wearableDynamics, boneMappings, targetAvatar, targetWearable))
            //{
            //    Debug.Log("Error applying bone mappings!");
            //}

            //Selection.activeGameObject = targetAvatar;
            //SceneView.FrameLastActiveSceneView();
        }

        private void DrawToolContentGUI()
        {
            _selectedInterface = GUILayout.Toolbar(_selectedInterface, new string[] { t._("tab_simple_mode"), t._("tab_advanced_mode") });

            DTEditorUtils.DrawHorizontalLine();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            if (_selectedInterface == 0)
            {
                DrawSimpleGUI();
            }
            else
            {
                DrawAdvancedGUI();
            }

            DTEditorUtils.DrawHorizontalLine();

            GUILayout.Label(t._("label_check_and_dress"), EditorStyles.boldLabel);

            var checkBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16
            };

            DrawReportResult();

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(_clothesToDress == null || _clothesToDress.name == ""); //|| illegalCharactersRegex.IsMatch(clothesToDress.name));
            if (GUILayout.Button(t._("button_check_and_preview"), checkBtnStyle, GUILayout.Height(40)))
            {
                GenerateMappingsAndApply(false);
                _dressNowConfirm = false;
                Debug.Log("[DressingToolsLegacy] Dress report generated with " + _report.LogEntries.Count + " log entries");
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(_report == null || _report.HasLogType(DTReportLogType.Error));
            if (GUILayout.Button(t._("button_test_now"), checkBtnStyle, GUILayout.Height(40)))
            {
                EditorApplication.EnterPlaymode();
            }
            EditorGUILayout.EndHorizontal();
            _dressNowConfirm = GUILayout.Toggle(_dressNowConfirm, t._("toggle_dress_declaration"));
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(_report == null || _report.HasLogType(DTReportLogType.Error) || !_dressNowConfirm);
            if (GUILayout.Button(t._("button_dress_now"), checkBtnStyle, GUILayout.Height(40)) &&
                EditorUtility.DisplayDialog(t._("label_tool_name"), t._("dialog_dress_confirmation_content"), t._("dialog_button_yes"), t._("dialog_button_no")))
            {
                GenerateMappingsAndApply(true);
                Debug.Log("[DressingToolsLegacy] Executed with " + _report.LogEntries.Count + " log entries");

                if (!_report.HasLogType(DTReportLogType.Error))
                {
                    EditorUtility.DisplayDialog(t._("label_tool_name"), t._("dialog_dress_completed_content"), t._("dialog_button_ok"));

                    // reset
                    _clothesToDress = null;
                    _report = null;
                }
                else
                {
                    EditorUtility.DisplayDialog(t._("label_tool_name"), t._("dialog_dress_failed_content"), t._("dialog_button_ok"));
                }
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Separator();

            DrawDressReportDetails();

            EditorGUILayout.EndScrollView();
        }

        void DrawHeaderButtonsGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Legacy v1 Editor", GUILayout.ExpandWidth(false));
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Settings 設定"))
            {
                SettingsWindow window = (SettingsWindow)GetWindow(typeof(SettingsWindow));
                window.titleContent = new GUIContent("DressingTools Settings 設定");
                window.Show();
            }

            GUILayout.EndHorizontal();
        }

        void OnGUI()
        {
            DrawHeaderButtonsGUI();
            DrawToolHeaderGUI();

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox(t._("helpbox_warn_exit_play_mode"), MessageType.Warning);
            }
            else
            {
                DrawToolContentGUI();
            }

            DrawToolFooterGUI();
        }
    }
}
