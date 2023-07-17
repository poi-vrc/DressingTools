using System.Collections.Generic;
using System.Text.RegularExpressions;
using Chocopoi.DressingTools.Applier.Default;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.Logging;
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

        private static readonly DTDefaultDresser DefaultDresser = new DTDefaultDresser();

        private static readonly DTDefaultApplier DefaultApplier = new DTDefaultApplier();

        private static AnimatorController testModeAnimationController;

        private int dynamicBoneOption = 0;

        private GameObject activeAvatar;

        private GameObject clothesToDress;

        private GameObject lastClothesToDress;

        private string newClothesName;

        //private bool useDefaultGeneratedPrefixSuffix = true;

        //private string prefixToBeAdded;

        //private string suffixToBeAdded;

        private bool useCustomArmatureObjectNames = false;

        private string avatarArmatureObjectName;

        private string clothesArmatureObjectName;

        private bool removeExistingPrefixSuffix = true;

        private bool groupBones = true;

        private bool groupRootObjects = true;

        private bool groupDynamics = true;

        private bool dressNowConfirm = false;

        private int selectedInterface = 0;

        private DTReport dresserReport = null;

        private DTReport applierReport = null;

        //private bool showStatisticsFoldout = false;

        private Vector2 scrollPos;

        private bool needClearDirty = false;

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
            testModeAnimationController = AssetDatabase.LoadAssetAtPath<AnimatorController>("Packages/com.chocopoi.vrc.dressingtools/Animations/TestModeAnimationController.controller");

            if (testModeAnimationController == null)
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
            if (needClearDirty)
            {
                needClearDirty = false;
                Repaint();
            }
        }

        private void FinishFetchOnlineVersion(DressingToolsUpdater.Manifest manifest)
        {
            //force redraw
            needClearDirty = true;
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

        private DTReportResult ConvertIntToDTReportResult(int result)
        {
            switch (result)
            {
                case -2:
                    return DTReportResult.InvalidSettings;
                case -1:
                    return DTReportResult.Incompatible;
                default:
                case 0:
                    return DTReportResult.Ok;
                case 1:
                    return DTReportResult.Compatible;
            }
        }

        private void DrawReportResult()
        {
            if (dresserReport == null || applierReport == null)
            {
                EditorGUILayout.HelpBox(t._("helpbox_warn_no_check_report"), MessageType.Warning);
                return;
            }

            //Result

            var min = System.Math.Min((int)dresserReport.Result, (int)applierReport.Result);
            var max = System.Math.Max((int)dresserReport.Result, (int)applierReport.Result);

            DTReportResult displayResult;
            if (min < 0)
            {
                displayResult = ConvertIntToDTReportResult(min);
            }
            else if (max > 0)
            {
                displayResult = ConvertIntToDTReportResult(max);
            }
            else
            {
                displayResult = DTReportResult.Ok;
            }

            switch (displayResult)
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

        private void DrawDressReportDetails()
        {
            if (dresserReport == null)
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

            EditorGUILayout.LabelField("Dresser", EditorStyles.boldLabel);

            DrawLogEntries(dresserReport.GetLogEntriesAsDictionary());

            EditorGUILayout.LabelField("Applier", EditorStyles.boldLabel);

            DrawLogEntries(applierReport.GetLogEntriesAsDictionary());
        }

        private DTDefaultDresserSettings MakeDressSettings()
        {
            DTDefaultDresserDynamicsOption dynamicsOption;
            switch (dynamicBoneOption)
            {
                default:
                case 0:
                    dynamicsOption = DTDefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint;
                    break;
                case 1:
                    dynamicsOption = DTDefaultDresserDynamicsOption.KeepDynamicsAndUseParentConstraintIfNecessary;
                    break;
                case 2:
                    dynamicsOption = DTDefaultDresserDynamicsOption.IgnoreTransform;
                    break;
                case 3:
                    dynamicsOption = DTDefaultDresserDynamicsOption.CopyDynamics;
                    break;
                case 4:
                    dynamicsOption = DTDefaultDresserDynamicsOption.IgnoreAll;
                    break;
            }

            return new DTDefaultDresserSettings()
            {
                targetAvatar = activeAvatar,
                targetWearable = clothesToDress,
                dynamicsOption = dynamicsOption,
                avatarArmatureName = avatarArmatureObjectName,
                wearableArmatureName = clothesArmatureObjectName
            };
        }

        private DTDefaultApplierSettings MakeApplierSettings()
        {
            return new DTDefaultApplierSettings()
            {
                removeExistingPrefixSuffix = removeExistingPrefixSuffix,
                groupBones = groupBones,
                groupRootObjects = groupRootObjects,
                groupDynamics = groupDynamics
            };
        }

        private void DrawNewClothesNameGUI()
        {
            if (clothesToDress != null && (clothesToDress.name == null || clothesToDress.name == "" || IllegalCharactersRegex.IsMatch(clothesToDress.name)))
            {
                //EditorGUILayout.HelpBox(t._("helpbox_error_clothes_name_illegal_characters_detected"), MessageType.Error);
                EditorGUILayout.HelpBox(t._("helpbox_warn_clothes_name_illegal_characters_detected_may_not_compatible_in_future_versions"), MessageType.Warning);
                if (newClothesName == null || newClothesName == "")
                {
                    newClothesName = "NewClothes_" + new System.Random().Next();
                }
                newClothesName = IllegalCharactersRegex.Replace(newClothesName, "");
            }

            if (newClothesName == null || lastClothesToDress != clothesToDress)
            {
                newClothesName = clothesToDress?.name;
            }
            lastClothesToDress = clothesToDress;

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(clothesToDress == null);
            newClothesName = EditorGUILayout.TextField(t._("text_new_clothes_name"), newClothesName);
            if (GUILayout.Button(t._("button_rename_clothes_name"), GUILayout.ExpandWidth(false)))
            {
                clothesToDress.name = newClothesName;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCustomArmatureNameGUI()
        {
            useCustomArmatureObjectNames = GUILayout.Toggle(useCustomArmatureObjectNames, t._("toggle_use_custom_armature_object_names"));

            if (!useCustomArmatureObjectNames)
            {
                avatarArmatureObjectName = "Armature";
                clothesArmatureObjectName = "Armature";
            }

            EditorGUI.BeginDisabledGroup(!useCustomArmatureObjectNames);
            EditorGUI.indentLevel = 1;
            avatarArmatureObjectName = EditorGUILayout.TextField(t._("text_custom_avatar_armature_object_name"), avatarArmatureObjectName);
            clothesArmatureObjectName = EditorGUILayout.TextField(t._("text_custom_clothes_armature_object_name"), clothesArmatureObjectName);
            EditorGUI.indentLevel = 0;
            EditorGUI.EndDisabledGroup();
        }

        private void DrawSimpleGUI()
        {
            GUILayout.Label(t._("label_setup"), EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(t._("helpbox_info_move_clothes_into_place"), MessageType.Info);

#if VRC_SDK_VRCSDK3
            if (activeAvatar == null)
            {
                VRC.SDKBase.VRC_AvatarDescriptor[] avatars = FindObjectsOfType<VRC.SDKBase.VRC_AvatarDescriptor>();
                foreach (var avatar in avatars)
                {
                    if (!avatar.name.StartsWith("DressingToolsPreview_"))
                    {
                        activeAvatar = avatar.gameObject;
                    }
                }
            }
#endif
            activeAvatar = (GameObject)EditorGUILayout.ObjectField(t._("object_active_avatar"), activeAvatar, typeof(GameObject), true);

            clothesToDress = (GameObject)EditorGUILayout.ObjectField(t._("object_clothes_to_dress"), clothesToDress, typeof(GameObject), true);

            DrawNewClothesNameGUI();

            groupBones = groupRootObjects = GUILayout.Toggle(groupBones | groupRootObjects, t._("toggle_group_bones_and_root_objects"));

            DrawCustomArmatureNameGUI();

            // simple mode defaults to group dynamics

            groupDynamics = true;

            // simple mode defaults to use generated prefix

            //useDefaultGeneratedPrefixSuffix = true;

            //if (clothesToDress != null)
            //{
            //    prefixToBeAdded = "";
            //    suffixToBeAdded = " (" + clothesToDress.name + ")";
            //}

            EditorGUILayout.Separator();

            if (clothesToDress != null)
            {
                EditorGUILayout.LabelField(t._("label_new_bone_name_preview", clothesToDress.name));
            }

            // simple mode defaults to handle dynamic bones automatically

            dynamicBoneOption = 0;

            EditorGUILayout.LabelField(t._("label_dynamic_bone_auto_handled"));
        }

        private void DrawAdvancedGUI()
        {
            GUILayout.Label(t._("label_select_avatar"), EditorStyles.boldLabel);

#if VRC_SDK_VRCSDK3
            if (activeAvatar == null)
            {
                VRC.SDKBase.VRC_AvatarDescriptor[] avatars = FindObjectsOfType<VRC.SDKBase.VRC_AvatarDescriptor>();
                foreach (var avatar in avatars)
                {
                    if (!avatar.name.StartsWith("DressingToolsPreview_"))
                    {
                        activeAvatar = avatar.gameObject;
                    }
                }
            }
#endif
            activeAvatar = (GameObject)EditorGUILayout.ObjectField(t._("object_active_avatar"), activeAvatar, typeof(GameObject), true);

            EditorGUILayout.Separator();

            GUILayout.Label(t._("label_select_clothes_to_dress"), EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(t._("helpbox_info_move_clothes_into_place"), MessageType.Info);

            clothesToDress = (GameObject)EditorGUILayout.ObjectField(t._("object_clothes_to_dress"), clothesToDress, typeof(GameObject), true);

            DrawNewClothesNameGUI();

            DrawCustomArmatureNameGUI();

            DTEditorUtils.DrawHorizontalLine();

            GUILayout.Label(t._("label_grouping_bones_root_objects_dynamics"), EditorStyles.boldLabel);

            EditorGUILayout.Separator();

            groupBones = GUILayout.Toggle(groupBones, t._("toggle_group_bones"));

            groupRootObjects = GUILayout.Toggle(groupRootObjects, t._("toggle_group_root_objects"));

            groupDynamics = GUILayout.Toggle(groupDynamics, t._("toggle_group_dynamics"));

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

            removeExistingPrefixSuffix = GUILayout.Toggle(removeExistingPrefixSuffix, t._("toggle_remove_existing_prefix_suffix_in_clothes_bone"));

            DTEditorUtils.DrawHorizontalLine();

            GUILayout.Label(t._("label_dynamic_bone"), EditorStyles.boldLabel);

            EditorGUILayout.Separator();

            GUILayout.Label(t._("label_dynamic_bone_if_in_avatar_bone"));

            var radioStyle = new GUIStyle(EditorStyles.radioButton)
            {
                wordWrap = true
            };

            dynamicBoneOption = GUILayout.SelectionGrid(dynamicBoneOption, new string[] {
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
            string avatarNewName = "DressingToolsPreview_" + activeAvatar.name;
            string clothesNewName = "DressingToolsPreview_" + clothesToDress.name;

            GameObject targetAvatar;
            GameObject targetWearable;

            if (write)
            {
                targetAvatar = activeAvatar;
                targetWearable = clothesToDress;
            }
            else
            {
                targetAvatar = Instantiate(activeAvatar);
                targetAvatar.name = avatarNewName;

                var newAvatarPosition = targetAvatar.transform.position;
                newAvatarPosition.x -= 20;
                targetAvatar.transform.position = newAvatarPosition;

                targetWearable = Instantiate(clothesToDress);
                targetWearable.name = clothesNewName;

                var newClothesPosition = targetWearable.transform.position;
                newClothesPosition.x -= 20;
                targetWearable.transform.position = newClothesPosition;

                var animator = targetAvatar.GetComponent<Animator>();

                //add animation controller
                if (animator != null)
                {
                    animator.runtimeAnimatorController = testModeAnimationController;
                }

                //add dummy focus sceneview script
                targetAvatar.AddComponent<DummyFocusSceneViewScript>();
            }

            dresserReport = DefaultDresser.Execute(MakeDressSettings(), out var boneMappings, out var objectMappings);

            var avatarDynamics = DTRuntimeUtils.ScanDynamics(targetAvatar);
            var wearableDynamics = DTRuntimeUtils.ScanDynamics(targetWearable);
            var applierSettings = MakeApplierSettings();

            applierReport = new DTReport();
            if (!DefaultApplier.ApplyBoneMappings(applierReport, applierSettings, clothesToDress.name, avatarDynamics, wearableDynamics, boneMappings, targetAvatar, targetWearable))
            {
                Debug.Log("Error applying bone mappings!");
            }

            if (!DefaultApplier.ApplyObjectMappings(applierReport, applierSettings, clothesToDress.name, objectMappings, targetAvatar, targetWearable))
            {
                Debug.Log("Error applying bone mappings!");
            }

            // finalize the report
            // TODO: move this to the getter of Result?
            var applierReportLogEntries = applierReport.GetLogEntriesAsDictionary();
            if (applierReportLogEntries.ContainsKey(DTReportLogType.Error))
            {
                applierReport.Result = DTReportResult.Incompatible;
            }
            else if (applierReportLogEntries.ContainsKey(DTReportLogType.Warning))
            {
                applierReport.Result = DTReportResult.Compatible;
            }
            else
            {
                applierReport.Result = DTReportResult.Ok;
            }

            Selection.activeGameObject = targetAvatar;
            SceneView.FrameLastActiveSceneView();
        }

        private void DrawToolContentGUI()
        {
            selectedInterface = GUILayout.Toolbar(selectedInterface, new string[] { t._("tab_simple_mode"), t._("tab_advanced_mode") });

            DTEditorUtils.DrawHorizontalLine();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            if (selectedInterface == 0)
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

            EditorGUI.BeginDisabledGroup(clothesToDress == null || clothesToDress.name == ""); //|| illegalCharactersRegex.IsMatch(clothesToDress.name));
            if (GUILayout.Button(t._("button_check_and_preview"), checkBtnStyle, GUILayout.Height(40)))
            {
                // clone if prefab

                if (PrefabUtility.IsPartOfAnyPrefab(clothesToDress))
                {
                    // clone the prefab
                    GameObject clonedPrefab = null;

                    // check if in scene or not
                    if (PrefabUtility.GetPrefabInstanceStatus(clothesToDress) != PrefabInstanceStatus.NotAPrefab)
                    {
                        // if in scene, we cannot clone with prefab connection or the overrides will be gone
                        clonedPrefab = Instantiate(clothesToDress);

                        // rename and disable original
                        clonedPrefab.name = clothesToDress.name;
                        clothesToDress.name += "-Prefab";
                        clothesToDress.SetActive(false);
                    }
                    else
                    {
                        // create prefab connection
                        clonedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(clothesToDress);
                        clonedPrefab.name = clothesToDress.name;

                        // unpack the outermost root of the prefab
                        PrefabUtility.UnpackPrefabInstance(clonedPrefab, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                    }

                    // set the clone to be the target clothes to dress
                    clothesToDress = clonedPrefab;
                }

                GenerateMappingsAndApply(false);
                dressNowConfirm = false;
                Debug.Log("[DressingTools] Dress report generated with result " + dresserReport.Result);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(dresserReport == null || dresserReport.Result < 0);
            if (GUILayout.Button(t._("button_test_now"), checkBtnStyle, GUILayout.Height(40)))
            {
                EditorApplication.EnterPlaymode();
            }
            EditorGUILayout.EndHorizontal();
            dressNowConfirm = GUILayout.Toggle(dressNowConfirm, t._("toggle_dress_declaration"));
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(dresserReport == null || applierReport == null || dresserReport.Result < 0 || applierReport.Result < 0 || !dressNowConfirm);
            if (GUILayout.Button(t._("button_dress_now"), checkBtnStyle, GUILayout.Height(40)) &&
                EditorUtility.DisplayDialog(t._("label_tool_name"), t._("dialog_dress_confirmation_content"), t._("dialog_button_yes"), t._("dialog_button_no")))
            {
                GenerateMappingsAndApply(true);
                Debug.Log("[DressingTools] Executed with result " + dresserReport.Result);

                if (dresserReport.Result >= 0 && applierReport.Result >= 0)
                {
                    EditorUtility.DisplayDialog(t._("label_tool_name"), t._("dialog_dress_completed_content"), t._("dialog_button_ok"));

                    // reset
                    clothesToDress = null;
                    dresserReport = null;
                }
                else
                {
                    EditorUtility.DisplayDialog(t._("label_tool_name"), t._("dialog_dress_failed_content", dresserReport.Result), t._("dialog_button_ok"));
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
