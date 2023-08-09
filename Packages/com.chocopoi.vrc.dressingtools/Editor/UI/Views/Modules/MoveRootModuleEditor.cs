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

        public event Action MoveToGameObjectFieldChange;
        public bool ShowSelectAvatarFirstHelpBox { get; set; }
        public bool IsGameObjectInvalid { get; set; }
        public GameObject MoveToGameObject { get => _moveToGameObject; set => _moveToGameObject = value; }

        private MoveRootModuleEditorPresenter _presenter;
        private IModuleEditorViewParent _parentView;
        private GameObject _moveToGameObject;

        public MoveRootModuleEditor(IModuleEditorViewParent parentView, DTWearableModuleBase target) : base(parentView, target)
        {
            _parentView = parentView;
            _presenter = new MoveRootModuleEditorPresenter(this, parentView, (MoveRootModule)target);
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
                GameObjectField("Move To", ref _moveToGameObject, true, MoveToGameObjectFieldChange);
            }
        }

        public override bool IsValid()
        {
            return !IsGameObjectInvalid;
        }
    }
}
