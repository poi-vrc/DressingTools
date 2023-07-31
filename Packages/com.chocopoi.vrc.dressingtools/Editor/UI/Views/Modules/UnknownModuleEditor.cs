using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views.Modules
{
    [CustomModuleEditor(typeof(UnknownModule))]
    internal class UnknownModuleEditor : ModuleEditor
    {
        private static Localization.I18n t = Localization.I18n.GetInstance();

        public UnknownModuleEditor(IWearableConfigView configView, DTWearableModuleBase target) : base(configView, target)
        {
        }

        public override void OnGUI()
        {
            var module = (UnknownModule)target;
            EditorGUILayout.HelpBox(t._("modules.unknown.helpBox.unknownModuleDetected", module.moduleTypeName), MessageType.Warning);
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
