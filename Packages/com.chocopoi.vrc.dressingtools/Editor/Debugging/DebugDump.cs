using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Chocopoi.DressingTools.DynamicsProxy;
using Chocopoi.DressingTools.Reporting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;

namespace Chocopoi.DressingTools.Debugging
{
    public static class DebugDump
    {
        [System.Serializable]
        public class DebugDumpInfoJson
        {
            public int version;
            public string tool_version;
            public string unity_version;
            public string operating_system;
            public string generated_utc_time;
        }

        [System.Serializable]
        public class DressReportDumpJson
        {
            public bool present;
            public string original_avatar_tree_csv;
            public string original_clothes_tree_csv;
            public string resultant_avatar_tree_csv;
            public string resultant_clothes_tree_csv;
            public string avatar_components_csv;
            public string clothes_components_csv;
            public int infos;
            public int warnings;
            public int errors;
            public int result;
        }

        [System.Serializable]
        public class ActiveSceneDumpJson
        {
            public string scene_tree_csv;
        }

        [System.Serializable]
        public class DependenciesDumpJson
        {
            public bool vrc_sdk3_present;
            public string vrc_sdk3_version;
            public bool vrcphysbone_present;
            public bool dynamicbone_present;
        }

        [System.Serializable]
        public class DebugDumpJson
        {
            public DebugDumpInfoJson debug_info;
            public DressReportDumpJson dress_report_dump;
            public ActiveSceneDumpJson active_scene_dump;
            public DependenciesDumpJson dependencies_dump;
        }

        private static Translation.I18n t = Translation.I18n.GetInstance();

        private static readonly int DumpVersion = 1;

        public static readonly string GameObjectCsvHeader = "parent_id,parent_name,child_id,child_name,child_active\n";
        public static readonly string ComponentsCsvHeader = "object_id,component_name\n";

        private static void PutDumpInfo(DebugDumpJson dump)
        {
            dump.debug_info = new DebugDumpInfoJson();
            dump.debug_info.operating_system = SystemInfo.operatingSystem;
            dump.debug_info.unity_version = Application.unityVersion;
            dump.debug_info.tool_version = DressingToolsUpdater.GetCurrentVersion()?.full_version_string;
            dump.debug_info.version = DumpVersion;
            dump.debug_info.generated_utc_time = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        }

        public static string GetVRCSDKVersion()
        {
            try
            {
                StreamReader reader = new StreamReader("Assets/VRCSDK/version.txt");
                string str = reader.ReadToEnd();
                // remove newline characters and trim string
                str = str.Trim().Replace("\n", "").Replace("\r", "");
                reader.Close();
                return str;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        private static void PutDependenciesDump(DebugDumpJson dump)
        {
            dump.dependencies_dump = new DependenciesDumpJson();

            // VRCSDK
            string vrcsdk_version = GetVRCSDKVersion();
            if (vrcsdk_version == null)
            {
                dump.dependencies_dump.vrc_sdk3_present = false;
            }
            else
            {
                dump.dependencies_dump.vrc_sdk3_present = true;
                dump.dependencies_dump.vrcphysbone_present = DressingUtils.FindType("VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone") != null;
                dump.dependencies_dump.vrc_sdk3_version = vrcsdk_version;
            }

            // DynamicBone
            dump.dependencies_dump.dynamicbone_present = DressingUtils.FindType("DynamicBone") != null;
        }

        public static string GenerateGameObjectTreeCsv(Transform parent, string csv = null)
        {
            if (csv == null)
            {
                csv = GameObjectCsvHeader;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);

                csv += string.Format("{0},{1},{2},{3},{4}\n", parent.GetInstanceID(), parent.name, child.GetInstanceID(), child.name, child.gameObject.activeSelf ? 1 : 0);

                csv = GenerateGameObjectTreeCsv(child, csv);
            }
            return csv;
        }

        public static List<ParentConstraint> FindParentConstraints(Transform parent, List<ParentConstraint> parentConstraints = null)
        {
            if (parentConstraints == null)
            {
                parentConstraints = new List<ParentConstraint>();
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);

                ParentConstraint[] comps = child.GetComponents<ParentConstraint>();
                parentConstraints.AddRange(comps);

                FindParentConstraints(child, parentConstraints);
            }

            return parentConstraints;

        }

        public static string GenerateSpecialComponentsCsv(List<DynamicBoneProxy> dynBones, List<PhysBoneProxy> physBones, List<ParentConstraint> parentConstraints)
        {
            string csv = ComponentsCsvHeader;

            foreach (DynamicBoneProxy bone in dynBones)
            {
                csv += string.Format("{0},{1}\n", bone.component.GetInstanceID(), "DynamicBone");
            }

            foreach (PhysBoneProxy bone in physBones)
            {
                csv += string.Format("{0},{1}\n", bone.component.GetInstanceID(), "VRCPhysBone");
            }

            if (parentConstraints != null)
            {
                foreach (ParentConstraint constraint in parentConstraints)
                {
                    csv += string.Format("{0},{1}\n", constraint.GetInstanceID(), "ParentConstraint");
                }
            }

            return csv;
        }

        private static void PutActiveSceneDump(DebugDumpJson dump)
        {
            dump.active_scene_dump = new ActiveSceneDumpJson();

            Scene scene = SceneManager.GetActiveScene();

            string obj_csv = GameObjectCsvHeader;
            GameObject[] objects = scene.GetRootGameObjects();

            foreach (GameObject obj in objects)
            {
                obj_csv += string.Format("{0},{1},{2},{3},{4}\n", -1, null, obj.GetInstanceID(), obj.name, obj.activeSelf ? 1 : 0);
                obj_csv = GenerateGameObjectTreeCsv(obj.transform, obj_csv);
            }

            dump.active_scene_dump.scene_tree_csv = obj_csv;
        }

        public static void GenerateDump(DressReport report)
        {
            // telling users about the content of the dump
            if (!EditorUtility.DisplayDialog(t._("label_tool_name"), t._("dialog_confirmation_of_content_of_debug_dump"), t._("dialog_button_yes"), t._("dialog_button_no")))
            {
                return;
            }

            // telling users about no previous report
            if (report == null && !EditorUtility.DisplayDialog(t._("label_tool_name"), t._("dialog_confirmation_of_no_previous_dress_report"), t._("dialog_button_yes"), t._("dialog_button_no")))
            {
                return;
            }

            // generate dump
            DebugDumpJson dump = new DebugDumpJson();

            PutDumpInfo(dump);
            PutDependenciesDump(dump);
            PutActiveSceneDump(dump);
            dump.dress_report_dump = report?.dressReportDump;

            // save file

            string dateTime = System.DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            string path = EditorUtility.SaveFilePanel("", "", "DressingTools_DebugDump_" + dateTime + ".json", "json");

            if (path.Length != 0)
            {
                try
                {
                    File.WriteAllText(path, JsonUtility.ToJson(dump));
                }
                catch (IOException e)
                {
                    UnityEngine.Debug.LogError(e);
                    EditorUtility.DisplayDialog("DressingTools", t._("dialog_preferences_unable_to_save_debug_dump_file", e.Message), "OK");
                }
            }
        }
    }
}
