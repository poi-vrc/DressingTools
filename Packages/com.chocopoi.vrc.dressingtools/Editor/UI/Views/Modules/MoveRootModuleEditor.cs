using System;
using Chocopoi.DressingTools.UI.Presenters.Modules;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views.Modules
{
    [CustomModuleEditor(typeof(MoveRootModule))]
    internal class MoveRootModuleEditor : ModuleEditor, IMoveRootModuleEditorView
    {
        private static Localization.I18n t = Localization.I18n.GetInstance();

        public event Action TargetAvatarOrWearableChange { add { configView_.TargetAvatarOrWearableChange += value; } remove { configView_.TargetAvatarOrWearableChange -= value; } }
        public event Action MoveToGameObjectFieldChange;
        public bool ShowSelectAvatarFirstHelpBox { get; set; }
        public bool IsGameObjectInvalid { get; set; }
        public GameObject MoveToGameObject { get => moveToGameObject_; set => moveToGameObject_ = value; }

        private MoveRootModuleEditorPresenter presenter_;
        private IWearableConfigView configView_;
        private GameObject moveToGameObject_;

        public MoveRootModuleEditor(IWearableConfigView configView, DTWearableModuleBase target) : base(configView, target)
        {
            configView_ = configView;
            presenter_ = new MoveRootModuleEditorPresenter(this, configView, (MoveRootModule)target);
            ShowSelectAvatarFirstHelpBox = true;
            IsGameObjectInvalid = true;
        }

        public override void OnGUI()
        {
            if (ShowSelectAvatarFirstHelpBox)
            {
                HelpBox("Please select an avatar first.", MessageType.Error);
            }
            else
            {
                if (IsGameObjectInvalid)
                {
                    HelpBox("The selected GameObject is not inside the avatar.", MessageType.Error);
                }
                GameObjectField("Move To", ref moveToGameObject_, true, MoveToGameObjectFieldChange);
            }
        }

        public override bool IsValid()
        {
            return !IsGameObjectInvalid;
        }
    }
}
