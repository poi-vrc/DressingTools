using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Modules
{
    [CustomModuleEditor(typeof(MoveRootModule))]
    public class MoveRootModuleEditor : ModuleEditor
    {
        private static Localization.I18n t = Localization.I18n.GetInstance();

        public MoveRootModuleEditor(DTWearableModuleBase target, DTWearableConfig config) : base(target, config)
        {
        }

        public override bool OnGUI(GameObject avatarGameObject, GameObject wearableGameObject)
        {
            var module = (MoveRootModule)target;

            if (avatarGameObject != null)
            {
                var lastObj = module.avatarPath != null ? avatarGameObject.transform.Find(module.avatarPath)?.gameObject : null;
                var newObj = (GameObject)EditorGUILayout.ObjectField("Move To", lastObj, typeof(GameObject), true);
                if (lastObj != newObj && DTRuntimeUtils.IsGrandParent(avatarGameObject.transform, newObj.transform))
                {
                    // renew path if changed
                    module.avatarPath = DTRuntimeUtils.GetRelativePath(newObj.transform, avatarGameObject.transform);
                    return true;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Please select an avatar first.", MessageType.Error);
            }

            return false;
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
