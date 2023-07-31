using System;
using Chocopoi.DressingTools.UI.Views.Modules;
using Chocopoi.DressingTools.UIBase;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;

namespace Chocopoi.DressingTools.UI.Views.Modules
{
    internal class ModuleEditor : EditorViewBase
    {
        public virtual string FriendlyName => GetType().Name;

        public bool foldout;

        protected IWearableConfigView configView;

        protected DTWearableModuleBase target;

        public ModuleEditor(IWearableConfigView configView, DTWearableModuleBase target)
        {
            this.configView = configView;
            this.target = target;
        }

        public override void OnGUI()
        {
            // TODO: default module editor?
            HelpBox("No editor available for this module.", MessageType.Error);
        }

        public virtual bool IsValid()
        {
            return true;
        }
    }
}
