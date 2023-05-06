using System.Text.RegularExpressions;
using Chocopoi.DressingTools.Debugging;
using Chocopoi.DressingTools.Reporting;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    public class DressingToolsWindow : EditorWindow
    {
        private static Translation.I18n t = Translation.I18n.GetInstance();

        private static DressingToolsUpdater.ParsedVersion currentVersion = DressingToolsUpdater.GetCurrentVersion();

        private static Regex illegalCharactersRegex = new Regex("[^a-zA-Z0-9_-]");

        private int dynamicBoneOption = 0;

        private VRC.SDKBase.VRC_AvatarDescriptor activeAvatar;

        private GameObject clothesToDress;

        private GameObject lastClothesToDress;

        private string newClothesName;

        private bool useDefaultGeneratedPrefixSuffix = true;

        private string prefixToBeAdded;

        private string suffixToBeAdded;

        private bool useCustomArmatureObjectNames = false;

        private string avatarArmatureObjectName;

        private string clothesArmatureObjectName;

        private bool removeExistingPrefixSuffix = true;

        private bool groupBones = true;

        private bool groupRootObjects = true;

        private bool groupDynamics = true;

        private bool dressNowConfirm = false;

        private int selectedInterface = 0;

        private DressReport dressReport = null;

        private bool showStatisticsFoldout = false;

        private Vector2 scrollPos;

        private bool needClearDirty = false;

        private bool updateAvailableFoldout = false;

        /// <summary>
        /// Initialize the Dressing Tool window
        /// </summary>
        [MenuItem("Tools/chocopoi/DressingTools", false, 0)]
        public static void Init()
        {
            DressingToolsWindow window = (DressingToolsWindow)GetWindow(typeof(DressingToolsWindow));
            window.titleContent = new GUIContent(t._("label_tool_name"));
            window.Show();
        }

        [MenuItem("Tools/chocopoi/Translations/Reload Dressing Tools Translations", false, 1)]
        public static void ReloadTranslations()
        {
            t.LoadTranslations(new string[] { "en", "zh", "jp", "kr", "fr" });
        }

        private void DrawToolHeaderGUI()
        {
            GUIStyle titleLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24
            };
            EditorGUILayout.LabelField(t._("label_tool_name") + " ❤️", titleLabelStyle, GUILayout.ExpandWidth(true), GUILayout.Height(30));

            EditorGUILayout.Separator();

            if (DressingToolsUpdater.IsUpdateChecked() && !DressingToolsUpdater.IsLastUpdateCheckErrored() && DressingToolsUpdater.IsUpdateAvailable())
            {
                DressingToolsUpdater.ManifestBranch branch = DressingToolsUpdater.GetBranchLatestVersion(Preferences.GetPreferences().app.update_branch);

                GUIStyle updateFoldoutStyle = new GUIStyle(EditorStyles.foldout);
                updateFoldoutStyle.normal.textColor = Color.red;
                updateFoldoutStyle.onNormal.textColor = Color.red;
                updateFoldoutStyle.fontStyle = FontStyle.Bold;

                if (updateAvailableFoldout = EditorGUILayout.Foldout(updateAvailableFoldout, "<" + t._("foldout_update_available") + "> " + DressingToolsUpdater.GetCurrentVersion().full_version_string + " -> " + branch.version, updateFoldoutStyle))
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    if (GUILayout.Button("> " + t._("button_download_from_booth"), EditorStyles.label))
                    {
                        Application.OpenURL(branch?.booth_url);
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    if (GUILayout.Button("> " + t._("button_download_from_github"), EditorStyles.label))
                    {
                        Application.OpenURL(branch?.github_url);
                    }
                    GUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.HelpBox(t._("label_header_tool_description"), MessageType.Info);

            DressingUtils.DrawHorizontalLine();
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
            DressingUtils.DrawHorizontalLine();

            GUILayout.Label(t._("label_footer_version", currentVersion?.full_version_string));

            if (DressingToolsUpdater.IsUpdateChecked())
            {
                if (DressingToolsUpdater.IsLastUpdateCheckErrored())
                {
                    GUILayout.Label(t._("label_could_not_check_update"));
                }
                else if (DressingToolsUpdater.IsUpdateAvailable())
                {
                    DressingToolsUpdater.ManifestBranch branch = DressingToolsUpdater.GetBranchLatestVersion(Preferences.GetPreferences().app.update_branch);

                    GUILayout.Label(t._("label_update_available", branch?.version));
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

        private void DrawDressReportResult()
        {
            if (dressReport == null)
            {
                EditorGUILayout.HelpBox(t._("helpbox_warn_no_check_report"), MessageType.Warning);
                return;
            }

            //Result

            switch (dressReport.result)
            {
                case DressCheckResult.INVALID_SETTINGS:
                    EditorGUILayout.HelpBox(t._("helpbox_error_check_result_invalid_settings"), MessageType.Error);
                    break;
                case DressCheckResult.INCOMPATIBLE:
                    EditorGUILayout.HelpBox(t._("helpbox_error_check_result_incompatible"), MessageType.Error);
                    break;
                case DressCheckResult.OK:
                    EditorGUILayout.HelpBox(t._("helpbox_info_check_result_ok"), MessageType.Info);
                    break;
                case DressCheckResult.COMPATIBLE:
                    EditorGUILayout.HelpBox(t._("helpbox_warn_check_result_compatible"), MessageType.Warning);
                    break;
            }
        }

        private void DrawDressReportDetails()
        {
            if (dressReport == null)
            {
                return;
            }

            showStatisticsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(showStatisticsFoldout, t._("foldout_dressing_statistics"));

            if (showStatisticsFoldout)
            {
                GUILayout.Label(t._("label_statistics_total_avatar_dynbones", dressReport.avatarDynBones.Count));

                GUILayout.Label(t._("label_statistics_total_avatar_physbones", dressReport.avatarPhysBones.Count));

                GUILayout.Label(t._("label_statistics_total_clothes_dynbones", dressReport.clothesDynBones.Count, dressReport.clothesOriginalDynBones.Count));

                GUILayout.Label(t._("label_statistics_total_clothes_physbones", dressReport.clothesPhysBones.Count, dressReport.clothesOriginalPhysBones.Count));

                GUILayout.Label(t._("label_statistics_total_clothes_objects", dressReport.clothesAllObjects.Count));

                GUILayout.Label(t._("label_statistics_total_clothes_mesh_data", dressReport.clothesMeshDataObjects.Count));
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField(t._("label_problems_detected"), EditorStyles.boldLabel);

            if (dressReport.infos == 0 && dressReport.warnings == 0 && dressReport.errors == 0)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField(t._("label_no_problems_found"));
                return;
            }

            // Errors

            if ((dressReport.errors & DressCheckCodeMask.Error.NO_ARMATURE_IN_AVATAR) == DressCheckCodeMask.Error.NO_ARMATURE_IN_AVATAR)
            {
                EditorGUILayout.HelpBox(t._("helpbox_error_no_armature_in_avatar"), MessageType.Error);
            }

            if ((dressReport.errors & DressCheckCodeMask.Error.NO_ARMATURE_IN_CLOTHES) == DressCheckCodeMask.Error.NO_ARMATURE_IN_CLOTHES)
            {
                EditorGUILayout.HelpBox(t._("helpbox_error_no_armature_in_clothes"), MessageType.Error);
            }

            if ((dressReport.errors & DressCheckCodeMask.Error.NULL_ACTIVE_AVATAR_OR_CLOTHES) == DressCheckCodeMask.Error.NULL_ACTIVE_AVATAR_OR_CLOTHES)
            {
                EditorGUILayout.HelpBox(t._("helpbox_error_null_avatar_or_clothes"), MessageType.Error);
            }

            if ((dressReport.errors & DressCheckCodeMask.Error.NO_BONES_IN_AVATAR_ARMATURE_FIRST_LEVEL) == DressCheckCodeMask.Error.NO_BONES_IN_AVATAR_ARMATURE_FIRST_LEVEL)
            {
                EditorGUILayout.HelpBox(t._("helpbox_error_no_bones_in_avatar_armature_first_level"), MessageType.Error);
            }

            if ((dressReport.errors & DressCheckCodeMask.Error.NO_BONES_IN_CLOTHES_ARMATURE_FIRST_LEVEL) == DressCheckCodeMask.Error.NO_BONES_IN_CLOTHES_ARMATURE_FIRST_LEVEL)
            {
                EditorGUILayout.HelpBox(t._("helpbox_error_no_bones_in_clothes_armature_first_level"), MessageType.Error);
            }

            if ((dressReport.errors & DressCheckCodeMask.Error.CLOTHES_IS_A_PREFAB) == DressCheckCodeMask.Error.CLOTHES_IS_A_PREFAB)
            {
                EditorGUILayout.HelpBox(t._("helpbox_error_clothes_is_prefab"), MessageType.Error);
            }

            if ((dressReport.errors & DressCheckCodeMask.Error.EXISTING_CLOTHES_DETECTED) == DressCheckCodeMask.Error.EXISTING_CLOTHES_DETECTED)
            {
                EditorGUILayout.HelpBox(t._("helpbox_error_existing_clothes_detected"), MessageType.Error);
            }

            if ((dressReport.errors & DressCheckCodeMask.Error.MISSING_SCRIPTS_DETECTED_IN_AVATAR) == DressCheckCodeMask.Error.MISSING_SCRIPTS_DETECTED_IN_AVATAR)
            {
                EditorGUILayout.HelpBox(t._("helpbox_error_missing_scripts_detected_in_avatar"), MessageType.Error);
            }

            if ((dressReport.errors & DressCheckCodeMask.Error.MISSING_SCRIPTS_DETECTED_IN_CLOTHES) == DressCheckCodeMask.Error.MISSING_SCRIPTS_DETECTED_IN_CLOTHES)
            {
                EditorGUILayout.HelpBox(t._("helpbox_error_missing_scripts_detected_in_clothes"), MessageType.Error);
            }

            if ((dressReport.errors & DressCheckCodeMask.Error.CLOTHES_INSIDE_AVATAR) == DressCheckCodeMask.Error.CLOTHES_INSIDE_AVATAR)
            {
                EditorGUILayout.HelpBox(t._("helpbox_error_clothes_inside_avatar"), MessageType.Error);
            }

            if ((dressReport.errors & DressCheckCodeMask.Error.AVATAR_INSIDE_CLOTHES) == DressCheckCodeMask.Error.AVATAR_INSIDE_CLOTHES)
            {
                EditorGUILayout.HelpBox(t._("helpbox_error_avatar_inside_clothes"), MessageType.Error);
            }

            // Warnings

            if ((dressReport.warnings & DressCheckCodeMask.Warn.MULTIPLE_BONES_IN_AVATAR_ARMATURE_FIRST_LEVEL) == DressCheckCodeMask.Warn.MULTIPLE_BONES_IN_AVATAR_ARMATURE_FIRST_LEVEL)
            {
                EditorGUILayout.HelpBox(t._("helpbox_warn_multiple_bones_in_avatar_armature_first_level"), MessageType.Warning);
            }

            if ((dressReport.warnings & DressCheckCodeMask.Warn.MULTIPLE_BONES_IN_CLOTHES_ARMATURE_FIRST_LEVEL) == DressCheckCodeMask.Warn.MULTIPLE_BONES_IN_CLOTHES_ARMATURE_FIRST_LEVEL)
            {
                EditorGUILayout.HelpBox(t._("helpbox_warn_multiple_bones_in_clothes_armature_first_level"), MessageType.Warning);
            }

            if ((dressReport.warnings & DressCheckCodeMask.Warn.BONES_NOT_MATCHING_IN_ARMATURE_FIRST_LEVEL) == DressCheckCodeMask.Warn.BONES_NOT_MATCHING_IN_ARMATURE_FIRST_LEVEL)
            {
                EditorGUILayout.HelpBox(t._("helpbox_warn_bones_not_matching_in_armature_first_level"), MessageType.Warning);
            }

            if ((dressReport.warnings & DressCheckCodeMask.Warn.MISSING_SCRIPTS_DETECTED_IN_AVATAR_WILL_BE_REMOVED) == DressCheckCodeMask.Warn.MISSING_SCRIPTS_DETECTED_IN_AVATAR_WILL_BE_REMOVED)
            {
                EditorGUILayout.HelpBox(t._("helpbox_warn_missing_scripts_detected_in_avatar_will_be_removed"), MessageType.Warning);
            }

            if ((dressReport.warnings & DressCheckCodeMask.Warn.MISSING_SCRIPTS_DETECTED_IN_CLOTHES_WILL_BE_REMOVED) == DressCheckCodeMask.Warn.MISSING_SCRIPTS_DETECTED_IN_CLOTHES_WILL_BE_REMOVED)
            {
                EditorGUILayout.HelpBox(t._("helpbox_warn_missing_scripts_detected_in_clothes_will_be_removed"), MessageType.Warning);
            }


            // Infos

            if ((dressReport.infos & DressCheckCodeMask.Info.NON_MATCHING_CLOTHES_BONE_KEPT_UNTOUCHED) == DressCheckCodeMask.Info.NON_MATCHING_CLOTHES_BONE_KEPT_UNTOUCHED)
            {
                EditorGUILayout.HelpBox(t._("helpbox_info_non_matching_clothes_bones_kept_untouched"), MessageType.Info);
            }

            if ((dressReport.infos & DressCheckCodeMask.Info.DYNAMIC_BONE_ALL_IGNORED) == DressCheckCodeMask.Info.DYNAMIC_BONE_ALL_IGNORED)
            {
                EditorGUILayout.HelpBox(t._("helpbox_info_dynamic_bone_all_ignored"), MessageType.Info);
            }

            if ((dressReport.infos & DressCheckCodeMask.Info.EXISTING_PREFIX_DETECTED_AND_REMOVED) == DressCheckCodeMask.Info.EXISTING_PREFIX_DETECTED_AND_REMOVED)
            {
                EditorGUILayout.HelpBox(t._("helpbox_info_existing_prefix_detected_and_removed"), MessageType.Info);
            }

            if ((dressReport.infos & DressCheckCodeMask.Info.EXISTING_PREFIX_DETECTED_NOT_REMOVED) == DressCheckCodeMask.Info.EXISTING_PREFIX_DETECTED_NOT_REMOVED)
            {
                EditorGUILayout.HelpBox(t._("helpbox_info_existing_prefix_detected_not_removed"), MessageType.Info);
            }

            if ((dressReport.infos & DressCheckCodeMask.Info.EXISTING_SUFFIX_DETECTED_AND_REMOVED) == DressCheckCodeMask.Info.EXISTING_SUFFIX_DETECTED_AND_REMOVED)
            {
                EditorGUILayout.HelpBox(t._("helpbox_info_existing_suffix_detected_and_removed"), MessageType.Info);
            }

            if ((dressReport.infos & DressCheckCodeMask.Info.EXISTING_SUFFIX_DETECTED_NOT_REMOVED) == DressCheckCodeMask.Info.EXISTING_SUFFIX_DETECTED_NOT_REMOVED)
            {
                EditorGUILayout.HelpBox(t._("helpbox_info_existing_suffix_detected_not_removed"), MessageType.Info);
            }

            if ((dressReport.infos & DressCheckCodeMask.Info.AVATAR_ARMATURE_OBJECT_GUESSED) == DressCheckCodeMask.Info.AVATAR_ARMATURE_OBJECT_GUESSED)
            {
                EditorGUILayout.HelpBox(t._("helpbox_info_avatar_armature_object_guessed"), MessageType.Info);
            }

            if ((dressReport.infos & DressCheckCodeMask.Info.CLOTHES_ARMATURE_OBJECT_GUESSED) == DressCheckCodeMask.Info.CLOTHES_ARMATURE_OBJECT_GUESSED)
            {
                EditorGUILayout.HelpBox(t._("helpbox_info_clothes_armature_object_guessed"), MessageType.Info);
            }

            if ((dressReport.infos & DressCheckCodeMask.Info.MULTIPLE_BONES_IN_AVATAR_ARMATURE_FIRST_LEVEL_WARNING_REMOVED) == DressCheckCodeMask.Info.MULTIPLE_BONES_IN_AVATAR_ARMATURE_FIRST_LEVEL_WARNING_REMOVED)
            {
                EditorGUILayout.HelpBox(t._("helpbox_info_multiple_bones_in_clothes_armature_first_level_warning_removed"), MessageType.Info);
            }
        }

        private DressSettings MakeDressSettings()
        {
            return new DressSettings
            {
                activeAvatar = activeAvatar,
                clothesToDress = clothesToDress,
                prefixToBeAdded = prefixToBeAdded,
                suffixToBeAdded = suffixToBeAdded,
                removeExistingPrefixSuffix = removeExistingPrefixSuffix,
                dynamicBoneOption = dynamicBoneOption,
                groupBones = groupBones,
                groupRootObjects = groupRootObjects,
                groupDynamics = groupDynamics,
                avatarArmatureObjectName = avatarArmatureObjectName,
                clothesArmatureObjectName = clothesArmatureObjectName
            };
        }

        private void DrawNewClothesNameGUI()
        {
            if (clothesToDress != null && (clothesToDress.name == null || clothesToDress.name == "" || illegalCharactersRegex.IsMatch(clothesToDress.name)))
            {
                //EditorGUILayout.HelpBox(t._("helpbox_error_clothes_name_illegal_characters_detected"), MessageType.Error);
                EditorGUILayout.HelpBox(t._("helpbox_warn_clothes_name_illegal_characters_detected_may_not_compatible_in_future_versions"), MessageType.Warning);
                if (newClothesName == null || newClothesName == "")
                {
                    newClothesName = "NewClothes_" + new System.Random().Next();
                }
                newClothesName = illegalCharactersRegex.Replace(newClothesName, "");
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

            if (activeAvatar == null)
            {
                VRC.SDKBase.VRC_AvatarDescriptor[] avatars = FindObjectsOfType<VRC.SDKBase.VRC_AvatarDescriptor>();
                foreach (VRC.SDKBase.VRC_AvatarDescriptor avatar in avatars)
                {
                    if (!avatar.name.StartsWith("DressingToolsPreview_"))
                    {
                        activeAvatar = avatar;
                    }
                }
            }
            activeAvatar = (VRC.SDKBase.VRC_AvatarDescriptor)EditorGUILayout.ObjectField(t._("object_active_avatar"), activeAvatar, typeof(VRC.SDKBase.VRC_AvatarDescriptor), true);

            clothesToDress = (GameObject)EditorGUILayout.ObjectField(t._("object_clothes_to_dress"), clothesToDress, typeof(GameObject), true);

            DrawNewClothesNameGUI();

            groupBones = groupRootObjects = GUILayout.Toggle(groupBones | groupRootObjects, t._("toggle_group_bones_and_root_objects"));

            DrawCustomArmatureNameGUI();

            // simple mode defaults to group dynamics

            groupDynamics = true;

            // simple mode defaults to use generated prefix

            useDefaultGeneratedPrefixSuffix = true;

            if (clothesToDress != null)
            {
                prefixToBeAdded = "";
                suffixToBeAdded = " (" + clothesToDress.name + ")";
            }

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

            if (activeAvatar == null)
            {
                activeAvatar = FindObjectOfType<VRC.SDKBase.VRC_AvatarDescriptor>();
            }
            activeAvatar = (VRC.SDKBase.VRC_AvatarDescriptor)EditorGUILayout.ObjectField(t._("object_active_avatar"), activeAvatar, typeof(VRC.SDKBase.VRC_AvatarDescriptor), true);

            EditorGUILayout.Separator();

            GUILayout.Label(t._("label_select_clothes_to_dress"), EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(t._("helpbox_info_move_clothes_into_place"), MessageType.Info);

            clothesToDress = (GameObject)EditorGUILayout.ObjectField(t._("object_clothes_to_dress"), clothesToDress, typeof(GameObject), true);

            DrawNewClothesNameGUI();

            DrawCustomArmatureNameGUI();

            DressingUtils.DrawHorizontalLine();

            GUILayout.Label(t._("label_grouping_bones_root_objects_dynamics"), EditorStyles.boldLabel);

            EditorGUILayout.Separator();

            groupBones = GUILayout.Toggle(groupBones, t._("toggle_group_bones"));

            groupRootObjects = GUILayout.Toggle(groupRootObjects, t._("toggle_group_root_objects"));

            groupDynamics = GUILayout.Toggle(groupDynamics, t._("toggle_group_dynamics"));

            DressingUtils.DrawHorizontalLine();

            GUILayout.Label(t._("label_prefix_suffix"), EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(t._("helpbox_info_prefix_suffix"), MessageType.Info);

            useDefaultGeneratedPrefixSuffix = GUILayout.Toggle(useDefaultGeneratedPrefixSuffix, t._("toggle_use_default_generated_prefix_suffix"));

            if (useDefaultGeneratedPrefixSuffix && clothesToDress != null)
            {
                prefixToBeAdded = "";
                suffixToBeAdded = " (" + clothesToDress.name + ")";
            }

            EditorGUI.BeginDisabledGroup(useDefaultGeneratedPrefixSuffix);
            EditorGUI.indentLevel = 1;
            prefixToBeAdded = EditorGUILayout.TextField(t._("text_prefix_to_be_added"), prefixToBeAdded);
            suffixToBeAdded = EditorGUILayout.TextField(t._("text_suffix_to_be_added"), suffixToBeAdded);
            EditorGUI.indentLevel = 0;
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Separator();

            removeExistingPrefixSuffix = GUILayout.Toggle(removeExistingPrefixSuffix, t._("toggle_remove_existing_prefix_suffix_in_clothes_bone"));

            DressingUtils.DrawHorizontalLine();

            GUILayout.Label(t._("label_dynamic_bone"), EditorStyles.boldLabel);

            EditorGUILayout.Separator();

            GUILayout.Label(t._("label_dynamic_bone_if_in_avatar_bone"));

            GUIStyle radioStyle = new GUIStyle(EditorStyles.radioButton)
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

        private void DrawToolContentGUI()
        {
            selectedInterface = GUILayout.Toolbar(selectedInterface, new string[] { t._("tab_simple_mode"), t._("tab_advanced_mode") });

            DressingUtils.DrawHorizontalLine();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            if (selectedInterface == 0)
            {
                DrawSimpleGUI();
            }
            else
            {
                DrawAdvancedGUI();
            }

            DressingUtils.DrawHorizontalLine();

            GUILayout.Label(t._("label_check_and_dress"), EditorStyles.boldLabel);

            GUIStyle checkBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16
            };

            DrawDressReportResult();

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

                // evaluate dressreport

                dressReport = DressReport.GenerateReport(MakeDressSettings());
                dressNowConfirm = false;
                Debug.Log("[DressingTools] Dress report generated with result " + dressReport.result + ", info code " + dressReport.infos + " warn code " + dressReport.warnings + " error code " + dressReport.errors);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(dressReport == null || dressReport.result < 0);
            if (GUILayout.Button(t._("button_test_now"), checkBtnStyle, GUILayout.Height(40)))
            {
                //EditorUtility.DisplayDialog(t._("label_tool_name"), t._("dialog_test_mode_not_implemented", activeAvatar?.name), "OK");
                EditorApplication.EnterPlaymode();
            }
            EditorGUILayout.EndHorizontal();
            dressNowConfirm = GUILayout.Toggle(dressNowConfirm, t._("toggle_dress_declaration"));
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(dressReport == null || dressReport.result < 0 || !dressNowConfirm);
            if (GUILayout.Button(t._("button_dress_now"), checkBtnStyle, GUILayout.Height(40)) &&
                EditorUtility.DisplayDialog(t._("label_tool_name"), t._("dialog_dress_confirmation_content"), t._("dialog_button_yes"), t._("dialog_button_no")))
            {
                dressReport = DressReport.Execute(MakeDressSettings(), true);
                Debug.Log("[DressingTools] Executed with result " + dressReport.result + ", info code " + dressReport.infos + " warn code " + dressReport.warnings + " error code " + dressReport.errors);

                if (dressReport.result >= 0)
                {
                    EditorUtility.DisplayDialog(t._("label_tool_name"), t._("dialog_dress_completed_content"), t._("dialog_button_ok"));

                    // reset
                    clothesToDress = null;
                    dressReport = null;
                }
                else
                {
                    EditorUtility.DisplayDialog(t._("label_tool_name"), t._("dialog_dress_failed_content", dressReport.result), t._("dialog_button_ok"));
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
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(t._("button_save_debug_dump")))
            {
                DebugDump.GenerateDump(dressReport);
            }

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
