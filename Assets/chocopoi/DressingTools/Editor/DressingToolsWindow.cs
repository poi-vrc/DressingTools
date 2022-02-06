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
            window.titleContent = new GUIContent("Dressing Tools");
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
                EditorGUILayout.HelpBox("No dress report has been generated yet. Press the \"Check now\" button to perform checks.", MessageType.Warning);
                return;
            }

            //Result

            switch (dressReport.result)
            {
                case DressCheckResult.INVALID_SETTINGS:
                    EditorGUILayout.HelpBox("Check Result: Invalid settings detected, please check your settings before continuing.", MessageType.Error);
                    break;
                case DressCheckResult.IMCOMPATIBLE:
                    EditorGUILayout.HelpBox("Check Result: Imcompatible, this tool cannot dress it automatically.", MessageType.Error);
                    break;
                case DressCheckResult.OK:
                    EditorGUILayout.HelpBox("Check Result: OK, it seems to perfectly fit your active avatar.", MessageType.Info);
                    break;
                case DressCheckResult.COMPATIBLE:
                    EditorGUILayout.HelpBox("Check Result: Compatible, but your clothes might not fully fit your active avatar.", MessageType.Warning);
                    break;
            }
        }

        private void DrawDressReportDetails()
        {
            if (dressReport == null)
            {
                return;
            }

            // Errors

            if ((dressReport.errors & DressCheckCodeMask.Error.NO_ARMATURE_IN_AVATAR) == DressCheckCodeMask.Error.NO_ARMATURE_IN_AVATAR)
            {
                EditorGUILayout.HelpBox("Error: No Armature in avatar", MessageType.Error);
            }

            if ((dressReport.errors & DressCheckCodeMask.Error.NO_ARMATURE_IN_CLOTHES) == DressCheckCodeMask.Error.NO_ARMATURE_IN_CLOTHES)
            {
                EditorGUILayout.HelpBox("Error: No Armature in clothes", MessageType.Error);
            }

            if ((dressReport.errors & DressCheckCodeMask.Error.NULL_ACTIVE_AVATAR_OR_CLOTHES) == DressCheckCodeMask.Error.NULL_ACTIVE_AVATAR_OR_CLOTHES)
            {
                EditorGUILayout.HelpBox("Error: No active avatar or clothes", MessageType.Error);
            }

            if ((dressReport.errors & DressCheckCodeMask.Error.NO_BONES_IN_AVATAR_ARMATURE_FIRST_LEVEL) == DressCheckCodeMask.Error.NO_BONES_IN_AVATAR_ARMATURE_FIRST_LEVEL)
            {
                EditorGUILayout.HelpBox("Error: No bones are detected in the first level of avatar armature.", MessageType.Error);
            }

            if ((dressReport.errors & DressCheckCodeMask.Error.NO_BONES_IN_CLOTHES_ARMATURE_FIRST_LEVEL) == DressCheckCodeMask.Error.NO_BONES_IN_CLOTHES_ARMATURE_FIRST_LEVEL)
            {
                EditorGUILayout.HelpBox("Error: No bones are detected in the first level of clothes armature.", MessageType.Error);
            }

            if ((dressReport.errors & DressCheckCodeMask.Error.CLOTHES_IS_A_PREFAB) == DressCheckCodeMask.Error.CLOTHES_IS_A_PREFAB)
            {
                EditorGUILayout.HelpBox("Error: Clothes cannot be a Prefab. Please \"Unpack it completely\" to turn it to be a normal GameObject.", MessageType.Error);
            }

            // Warnings

            if ((dressReport.warnings & DressCheckCodeMask.Warn.MULTIPLE_BONES_IN_AVATAR_ARMATURE_FIRST_LEVEL) == DressCheckCodeMask.Warn.MULTIPLE_BONES_IN_AVATAR_ARMATURE_FIRST_LEVEL)
            {
                EditorGUILayout.HelpBox("Warning: Multiple bones detected in the first level of avatar armature.", MessageType.Warning);
            }

            if ((dressReport.warnings & DressCheckCodeMask.Warn.MULTIPLE_BONES_IN_CLOTHES_ARMATURE_FIRST_LEVEL) == DressCheckCodeMask.Warn.MULTIPLE_BONES_IN_CLOTHES_ARMATURE_FIRST_LEVEL)
            {
                EditorGUILayout.HelpBox("Warning: Multiple bones detected in the first level of clothes armature.", MessageType.Warning);
            }

            if ((dressReport.warnings & DressCheckCodeMask.Warn.BONES_NOT_MATCHING_IN_ARMATURE_FIRST_LEVEL) == DressCheckCodeMask.Warn.BONES_NOT_MATCHING_IN_ARMATURE_FIRST_LEVEL)
            {
                EditorGUILayout.HelpBox("Warning: Some bones are not matching in the first level of armature. They will not be moved.", MessageType.Warning);
            }

            // Infos

            if ((dressReport.infos & DressCheckCodeMask.Info.NON_MATCHING_CLOTHES_BONE_KEPT_UNTOUCHED) == DressCheckCodeMask.Info.NON_MATCHING_CLOTHES_BONE_KEPT_UNTOUCHED)
            {
                EditorGUILayout.HelpBox("Info: Non-matching clothes bone will be kept untouched in their original parent.", MessageType.Info);
            }

            if ((dressReport.infos & DressCheckCodeMask.Info.DYNAMIC_BONE_ALL_IGNORED) == DressCheckCodeMask.Info.DYNAMIC_BONE_ALL_IGNORED)
            {
                EditorGUILayout.HelpBox("Info: All matching dynamic bones are ignored. It may cause unexpected behaviour.", MessageType.Info);
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

        private void DrawToolContentGUI()
        {
            selectedInterface = GUILayout.Toolbar(selectedInterface, new string[] { "Simple Mode", "Advanced Mode" });

            DrawHorizontalLine();

            GUILayout.Label(t._("label_select_avatar"), EditorStyles.boldLabel);

            if (activeAvatar == null)
            {
                activeAvatar = FindObjectOfType<VRC.SDKBase.VRC_AvatarDescriptor>();
            }
            activeAvatar = (VRC.SDKBase.VRC_AvatarDescriptor)EditorGUILayout.ObjectField("Active Avatar", activeAvatar, typeof(VRC.SDKBase.VRC_AvatarDescriptor), true);

            EditorGUILayout.Separator();

            GUILayout.Label(t._("label_select_clothes_to_dress"), EditorStyles.boldLabel);

            clothesToDress = (GameObject) EditorGUILayout.ObjectField(t._("label_clothes_to_dress"), clothesToDress, typeof(GameObject), true);

            EditorGUILayout.Separator();

            GUILayout.Label(t._("label_setup_prefix_suffix"), EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(t._("label_helpbox_prefix_suffix"), MessageType.Info);

            useDefaultGeneratedPrefixSuffix = GUILayout.Toggle(useDefaultGeneratedPrefixSuffix, t._("toggle_use_default_generated_prefix_suffix"));

            if (useDefaultGeneratedPrefixSuffix && clothesToDress != null)
            {
                prefixToBeAdded = "";
                suffixToBeAdded = " (" + clothesToDress.name + ")";
            }

            EditorGUI.BeginDisabledGroup(useDefaultGeneratedPrefixSuffix);
            EditorGUI.indentLevel = 1;
            prefixToBeAdded = EditorGUILayout.TextField(t._("label_prefix_to_be_added"), prefixToBeAdded);
            suffixToBeAdded = EditorGUILayout.TextField(t._("label_suffix_to_be_added"), suffixToBeAdded);
            EditorGUI.indentLevel = 0;
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Separator();

            detectAndRemoveExistingSuffix = GUILayout.Toggle(detectAndRemoveExistingSuffix, t._("toggle_detect_and_remove_existing_suffix"));

            EditorGUILayout.Separator();

            GUILayout.Label(t._("label_dynamic_bones"), EditorStyles.boldLabel);

            GUILayout.Label(t._("In cases of dynamic bones of same avatar bone in clothes:"));

            GUIStyle radioStyle = new GUIStyle(EditorStyles.radioButton)
            {
                wordWrap = true
            };

            dynamicBoneOption = GUILayout.SelectionGrid(dynamicBoneOption, new string[] {
                " Remove and use parent constraints pointing to avatar for all (Preferred)",
                " Keep clothes dynamic bones and use parent constraints if necessary",
                " Create and use GameObject child on avatar (Legacy)",
                " Ignore all"
            }, 1, radioStyle, GUILayout.ExpandHeight(true), GUILayout.MaxHeight(150));

            EditorGUILayout.Separator();

            GUILayout.Label(t._("label_perform_checks_and_dress"), EditorStyles.boldLabel);

            GUIStyle checkBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16
            };

            DrawDressReportResult();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(t._("button_check_now"), checkBtnStyle, GUILayout.Height(40)))
            {
                dressReport = DressReport.GenerateReport(MakeDressSettings());
                dressNowConfirm = false;
                Debug.Log("Dress report generated with result " + dressReport.result + ", info code " + dressReport.infos + " warn code " + dressReport.warnings + " error code " + dressReport.errors);
            }

            EditorGUI.BeginDisabledGroup(dressReport == null || dressReport.result < 0);
            if (GUILayout.Button(t._("button_test_now"), checkBtnStyle, GUILayout.Height(40)))
            {

            }
            EditorGUILayout.EndHorizontal();
            dressNowConfirm = GUILayout.Toggle(dressNowConfirm, "I have confirmed that the avatar fits well with no problems.");
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(dressReport == null || dressReport.result < 0 || !dressNowConfirm);
            if (GUILayout.Button(t._("button_dress_now"), checkBtnStyle, GUILayout.Height(40)) &&
                EditorUtility.DisplayDialog("Dressing Tools", "Are you sure to proceed? This cannot be undone.", "Yes", "No"))
            {
                dressReport = DressReport.Execute(MakeDressSettings(), true);
                Debug.Log("Executed with result " + dressReport.result + ", info code " + dressReport.infos + " warn code " + dressReport.warnings + " error code " + dressReport.errors);
            }
            EditorGUI.EndDisabledGroup();

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