using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Modules
{
    [CustomModuleEditor(typeof(UnknownModule))]
    public class UnknownModuleEditor : ModuleEditor
    {
        private static Localization.I18n t = Localization.I18n.GetInstance();

        public UnknownModuleEditor(DTWearableModuleBase target, DTWearableConfig config) : base(target, config)
        {
        }

        public override bool OnGUI(GameObject avatarGameObject, GameObject wearableGameObject)
        {
            var module = (UnknownModule)target;
            EditorGUILayout.HelpBox(t._("modules.unknown.helpBox.unknownModuleDetected", module.moduleTypeName), MessageType.Warning);
            return false;
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
