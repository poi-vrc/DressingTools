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

        protected IModuleEditorViewParent parentView;

        protected DTWearableModuleBase target;

        public ModuleEditor(IModuleEditorViewParent parentView, DTWearableModuleBase target)
        {
            this.parentView = parentView;
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
