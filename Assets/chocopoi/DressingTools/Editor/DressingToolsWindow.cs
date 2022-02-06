using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    public class DressingToolsWindow : EditorWindow
    {
        private static I18n t = I18n.GetInstance();

        private static readonly string TOOL_VERSION = GetToolVersion();

        private int selectedLang = 0;

        private int dynamicBoneOption = 0;

        private VRC.SDKBase.VRC_AvatarDescriptor activeAvatar;

        private GameObject clothesToDress;

        private bool useDefaultGeneratedPrefixSuffix = true;

        private string prefixToBeAdded;

        private string suffixToBeAdded;

        private bool detectAndRemoveExistingSuffix = true;

        private bool dressNowConfirm = false;

        private int selectedInterface = 0;

        private DressReport dressReport = null;

        /// <summary>
        /// Initialize the Dressing Tool window
        /// </summary>
        [MenuItem("chocopoi/Dressing Tools")]
        public static void Init()
        {
            DressingToolsWindow window = (DressingToolsWindow)GetWindow(typeof(DressingToolsWindow));
            window.titleContent = new GUIContent(t._("label_tool_name"));
            window.Show();
        }

        /// <summary>
        /// Fetch the tool version text from Assets
        /// </summary> 
        /// <returns></returns>
        private static string GetToolVersion()
        {
            StreamReader reader = new StreamReader("Assets/chocopoi/DressingTools/version.txt");
            string str = reader.ReadToEnd();
            reader.Close();
            return str;
        }

        /// <summary>
        /// Draws a horizontal line
        /// Reference: https://forum.unity.com/threads/horizontal-line-in-editor-window.520812/#post-3416790
        /// </summary>
        /// <param name="i_height">The line height</param>
        void DrawHorizontalLine(int i_height = 1)
        {
            EditorGUILayout.Separator();
            Rect rect = EditorGUILayout.GetControlRect(false, i_height);
            rect.height = i_height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Separator();
        }

        private void DrawLanguageSelectorGUI()
        {
            if (GUILayout.Button("Reload translations"))
            {
                t.LoadTranslations(new string[] { "en", "zh", "jp" });
            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Language 語言 言語:");
            selectedLang = GUILayout.Toolbar(selectedLang, new string[] { "EN", "中", "JP" });

            if (selectedLang == 0)
            {
                t.SetLocale("en");
            }
            else if (selectedLang == 1)
            {
                t.SetLocale("zh");
            }
            else if (selectedLang == 2)
            {
                t.SetLocale("jp");
            }
            GUILayout.EndHorizontal();
        }

        private void DrawToolHeaderGUI()
        {
            GUIStyle titleLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24
            };
            EditorGUILayout.LabelField("Dressing Tools", titleLabelStyle, GUILayout.ExpandWidth(true), GUILayout.Height(30));

            EditorGUILayout.Separator();

            EditorGUILayout.HelpBox(t._("label_header_tool_description"), MessageType.Info);

            DrawHorizontalLine();
        }

        private void DrawToolFooterGUI()
        {
            DrawHorizontalLine();

            GUILayout.Label(t._("label_footer_version", TOOL_VERSION));
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

            // Infos

            if ((dressReport.infos & DressCheckCodeMask.Info.NON_MATCHING_CLOTHES_BONE_KEPT_UNTOUCHED) == DressCheckCodeMask.Info.NON_MATCHING_CLOTHES_BONE_KEPT_UNTOUCHED)
            {
                EditorGUILayout.HelpBox(t._("helpbox_info_non_matching_clothes_bones_kept_untouched"), MessageType.Info);
            }

            if ((dressReport.infos & DressCheckCodeMask.Info.DYNAMIC_BONE_ALL_IGNORED) == DressCheckCodeMask.Info.DYNAMIC_BONE_ALL_IGNORED)
            {
                EditorGUILayout.HelpBox(, MessageType.Info);
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
        }

        private DressSettings MakeDressSettings()
        {
            return new DressSettings
            {
                activeAvatar = activeAvatar,
                clothesToDress = clothesToDress,
                prefixToBeAdded = prefixToBeAdded,
                suffixToBeAdded = suffixToBeAdded,
                detectAndRemoveExistingSuffix = detectAndRemoveExistingSuffix,
                dynamicBoneOption = dynamicBoneOption
            };
        }

        private void DrawSimpleGUI()
        {
            GUILayout.Label(t._("label_setup"), EditorStyles.boldLabel);

            if (activeAvatar == null)
            {
                activeAvatar = FindObjectOfType<VRC.SDKBase.VRC_AvatarDescriptor>();
            }
            activeAvatar = (VRC.SDKBase.VRC_AvatarDescriptor)EditorGUILayout.ObjectField("Active Avatar", activeAvatar, typeof(VRC.SDKBase.VRC_AvatarDescriptor), true);

            clothesToDress = (GameObject)EditorGUILayout.ObjectField(t._("object_clothes_to_dress"), clothesToDress, typeof(GameObject), true);

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

            clothesToDress = (GameObject)EditorGUILayout.ObjectField(t._("object_clothes_to_dress"), clothesToDress, typeof(GameObject), true);

            DrawHorizontalLine();

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

            detectAndRemoveExistingSuffix = GUILayout.Toggle(detectAndRemoveExistingSuffix, t._("toggle_remove_existing_suffix_in_clothes_bone"));

            DrawHorizontalLine();

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
                " " + t._("radio_db_ignore_all")
            }, 1, radioStyle, GUILayout.ExpandHeight(true), GUILayout.MaxHeight(150));
        }

        private void DrawToolContentGUI()
        {
            selectedInterface = GUILayout.Toolbar(selectedInterface, new string[] { t._("tab_simple_mode"), t._("tab_advanced_mode") });

            DrawHorizontalLine();

            if (selectedInterface == 0)
            {
                DrawSimpleGUI();
            } else
            {
                DrawAdvancedGUI();
            }

            DrawHorizontalLine();

            GUILayout.Label(t._("label_check_and_dress"), EditorStyles.boldLabel);

            GUIStyle checkBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16
            };

            DrawDressReportResult();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(t._("button_check_and_preview"), checkBtnStyle, GUILayout.Height(40)))
            {
                dressReport = DressReport.GenerateReport(MakeDressSettings());
                dressNowConfirm = false;
                Debug.Log("Dress report generated with result " + dressReport.result + ", info code " + dressReport.infos + " warn code " + dressReport.warnings + " error code " + dressReport.errors);
            }

            EditorGUI.BeginDisabledGroup(dressReport == null || dressReport.result < 0);
            if (GUILayout.Button(t._("button_test_now"), checkBtnStyle, GUILayout.Height(40)))
            {
                EditorUtility.DisplayDialog(t._("label_tool_name"), "Not implemented yet.", "OK");
            }
            EditorGUILayout.EndHorizontal();
            dressNowConfirm = GUILayout.Toggle(dressNowConfirm, t._("toggle_dress_declaration"));
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(dressReport == null || dressReport.result < 0 || !dressNowConfirm);
            if (GUILayout.Button(t._("button_dress_now"), checkBtnStyle, GUILayout.Height(40)) &&
                EditorUtility.DisplayDialog(t._("label_tool_name"), t._("dialog_dress_confirmation_content"), t._("dialog_button_yes"), t._("dialog_button_no")))
            {
                dressReport = DressReport.Execute(MakeDressSettings(), true);
                Debug.Log("Executed with result " + dressReport.result + ", info code " + dressReport.infos + " warn code " + dressReport.warnings + " error code " + dressReport.errors);

                if (dressReport.result >= 0)
                {
                    EditorUtility.DisplayDialog(t._("label_tool_name"), t._("dialog_dress_completed_content"), t._("dialog_button_ok"));

                    // reset
                    clothesToDress = null;
                    dressReport = null;
                } else
                {
                    EditorUtility.DisplayDialog(t._("label_tool_name"), t._("dialog_dress_failed_content", dressReport.result), t._("dialog_button_ok"));
                }
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Separator();

            DrawDressReportDetails();
        }

        /// <summary>
        /// Renders the window
        /// </summary>
        void OnGUI()
        {
            DrawLanguageSelectorGUI();
            DrawToolHeaderGUI();
            DrawToolContentGUI();
            DrawToolFooterGUI();
        }
    }
}