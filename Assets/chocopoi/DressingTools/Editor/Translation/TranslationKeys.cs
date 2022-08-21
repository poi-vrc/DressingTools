using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.Translation
{
    [System.Serializable]
    public class TranslationKeys
    {
        public string label_tool_name;
        public string label_header_tool_description;
        public string label_footer_version;

        public string label_checking_for_update;
        public string label_up_to_date;
        public string label_update_available;
        public string label_could_not_check_update;

        public string tab_simple_mode;
        public string tab_advanced_mode;

        //Simple Mode
        public string label_setup;
        public string label_new_bone_name_preview;
        public string label_dynamic_bone_auto_handled;

        //Advanced Mode
        public string label_select_avatar;
        public string object_active_avatar;

        public string label_select_clothes_to_dress;
        public string object_clothes_to_dress;

        public string label_prefix_suffix;
        public string helpbox_info_prefix_suffix;

        public string toggle_use_default_generated_prefix_suffix;
        public string text_prefix_to_be_added;
        public string text_suffix_to_be_added;
        public string toggle_remove_existing_prefix_suffix_in_clothes_bone;

        public string label_dynamic_bone;
        public string label_dynamic_bone_if_in_avatar_bone;
        public string radio_db_remove_and_parent_const;
        public string radio_db_keep_clothes_and_parent_const_if_need;
        public string radio_db_create_child_and_exclude;
        public string radio_db_copy_dyn_bone_to_clothes;
        public string radio_db_ignore_all;

        public string label_check_and_dress;
        public string button_check_and_preview;
        public string button_test_now;
        public string toggle_dress_declaration;
        public string button_dress_now;

        public string dialog_dress_confirmation_content;
        public string dialog_dress_completed_content;
        public string dialog_dress_failed_content;
        public string dialog_button_yes;
        public string dialog_button_no;
        public string dialog_button_ok;

        //Check report
        public string helpbox_warn_no_check_report;
        public string helpbox_error_check_result_invalid_settings;
        public string helpbox_error_check_result_incompatible;
        public string helpbox_info_check_result_ok;
        public string helpbox_warn_check_result_compatible;

        public string label_problems_detected;
        public string label_no_problems_found;
        public string helpbox_error_no_armature_in_avatar;
        public string helpbox_error_no_armature_in_clothes;
        public string helpbox_error_null_avatar_or_clothes;
        public string helpbox_error_no_bones_in_avatar_armature_first_level;
        public string helpbox_error_no_bones_in_clothes_armature_first_level;
        public string helpbox_error_clothes_is_prefab;
        public string helpbox_warn_multiple_bones_in_avatar_armature_first_level;
        public string helpbox_warn_multiple_bones_in_clothes_armature_first_level;
        public string helpbox_warn_bones_not_matching_in_armature_first_level;
        public string helpbox_info_non_matching_clothes_bones_kept_untouched;
        public string helpbox_info_dynamic_bone_all_ignored;
        public string helpbox_info_existing_prefix_detected_and_removed;
        public string helpbox_info_existing_prefix_detected_not_removed;
        public string helpbox_info_existing_suffix_detected_and_removed;
        public string helpbox_info_existing_suffix_detected_not_removed;
        public string helpbox_info_multiple_bones_in_clothes_armature_first_level_warning_removed;
        public string helpbox_info_avatar_armature_object_guessed;
        public string helpbox_info_clothes_armature_object_guessed;

        public string helpbox_info_dyn_bone_config_details;
        public string helpbox_info_move_clothes_into_place;
        public string helpbox_warn_exit_play_mode;
        public string dialog_test_mode_not_implemented;

        public string label_download;

        public string text_new_clothes_name;
        public string button_rename_clothes_name;
        public string helpbox_error_clothes_name_illegal_characters_detected;
        public string foldout_dressing_statistics;
        public string label_statistics_total_avatar_dynbones;
        public string label_statistics_total_avatar_physbones;
        public string label_statistics_total_clothes_dynbones;
        public string label_statistics_total_clothes_physbones;
        public string label_statistics_total_clothes_objects;
        public string label_statistics_total_clothes_mesh_data;

        public string label_grouping_bones_root_objects_dynamics;
        public string toggle_group_bones;
        public string toggle_group_root_objects;
        public string toggle_group_dynamics;
        public string toggle_group_bones_and_root_objects;

        public string toggle_use_custom_armature_object_names;
        public string text_custom_avatar_armature_object_name;
        public string text_custom_clothes_armature_object_name;

        public string helpbox_error_missing_scripts_detected_in_avatar;
        public string helpbox_error_missing_scripts_detected_in_clothes;
    }
}
