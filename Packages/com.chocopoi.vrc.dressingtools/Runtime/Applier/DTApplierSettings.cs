using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Applier
{
    public class DTApplierSettings
    {
        public bool removeExistingPrefixSuffix;

        public bool groupBones;

        public bool groupRootObjects;

        public bool groupDynamics;

        public DTApplierSettings()
        {
            removeExistingPrefixSuffix = true;
            groupBones = true;
            groupRootObjects = true;
            groupDynamics = true;
        }

        public virtual bool DrawEditorGUI()
        {
            var newRemoveExistingPrefixSuffix = EditorGUILayout.ToggleLeft("Remove existing prefixes and suffixes", removeExistingPrefixSuffix);
            var newGroupBones = EditorGUILayout.ToggleLeft("Group bones", groupBones);
            var newGroupRootObjects = EditorGUILayout.ToggleLeft("Group root objects", groupRootObjects);
            var newGroupDynamics = EditorGUILayout.ToggleLeft("Group dynamics", groupDynamics);

            var modified = newRemoveExistingPrefixSuffix != removeExistingPrefixSuffix || newGroupBones != groupBones || newGroupRootObjects != groupRootObjects || newGroupDynamics != groupDynamics;

            removeExistingPrefixSuffix = newRemoveExistingPrefixSuffix;
            groupBones = newGroupBones;
            groupRootObjects = newGroupRootObjects;
            groupDynamics = newGroupDynamics;

            return modified;
        }
    }
}
