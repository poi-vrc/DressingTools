using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Applier
{
    public class DTApplierSettings
    {
        public bool removeExistingPrefixSuffix;

        public bool groupBones;

        public bool groupDynamics;

        public DTApplierSettings()
        {
            removeExistingPrefixSuffix = true;
            groupBones = true;
            groupDynamics = true;
        }

#if UNITY_EDITOR
        public virtual bool DrawEditorGUI()
        {
            var newRemoveExistingPrefixSuffix = EditorGUILayout.ToggleLeft("Remove existing prefixes and suffixes", removeExistingPrefixSuffix);
            var newGroupBones = EditorGUILayout.ToggleLeft("Group bones", groupBones);
            var newGroupDynamics = EditorGUILayout.ToggleLeft("Group dynamics", groupDynamics);

            var modified = newRemoveExistingPrefixSuffix != removeExistingPrefixSuffix || newGroupBones != groupBones || newGroupDynamics != groupDynamics;

            removeExistingPrefixSuffix = newRemoveExistingPrefixSuffix;
            groupBones = newGroupBones;
            groupDynamics = newGroupDynamics;

            return modified;
        }
#endif
    }
}
