using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters.Modules
{
    internal class MoveRootModuleEditorPresenter
    {
        private IMoveRootModuleEditorView view_;
        private IWearableConfigView configView_;
        private MoveRootModule module_;

        public MoveRootModuleEditorPresenter(IMoveRootModuleEditorView view, IWearableConfigView configView, MoveRootModule module)
        {
            view_ = view;
            configView_ = configView;
            module_ = module;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            view_.Load += OnLoad;
            view_.Unload += OnUnload;

            view_.TargetAvatarOrWearableChange += OnTargetAvatarOrWearableChange;
            view_.MoveToGameObjectFieldChange += OnMoveToGameObjectFieldChange;
        }

        private void UnsubscribeEvents()
        {
            view_.Load -= OnLoad;
            view_.Unload -= OnUnload;

            view_.TargetAvatarOrWearableChange -= OnTargetAvatarOrWearableChange;
            view_.MoveToGameObjectFieldChange -= OnMoveToGameObjectFieldChange;
        }

        private void OnTargetAvatarOrWearableChange()
        {
            ApplyMoveToGameObjectFieldChanges();
            UpdateView();
        }

        private void UpdateView()
        {
            if (configView_.TargetAvatar != null)
            {
                view_.ShowSelectAvatarFirstHelpBox = false;
                view_.MoveToGameObject = module_.avatarPath != null ? configView_.TargetAvatar.transform.Find(module_.avatarPath)?.gameObject : null;
            }
            else
            {
                view_.ShowSelectAvatarFirstHelpBox = true;
            }
        }

        private void OnMoveToGameObjectFieldChange()
        {
            ApplyMoveToGameObjectFieldChanges();
        }

        private void ApplyMoveToGameObjectFieldChanges()
        {
            if (configView_.TargetAvatar != null && view_.MoveToGameObject != null && DTRuntimeUtils.IsGrandParent(configView_.TargetAvatar.transform, view_.MoveToGameObject.transform))
            {
                view_.IsGameObjectInvalid = false;

                // renew path if valid
                module_.avatarPath = DTRuntimeUtils.GetRelativePath(view_.MoveToGameObject.transform, configView_.TargetAvatar.transform);
            }
            else
            {
                // show helpbox that the path is invalid
                view_.IsGameObjectInvalid = true;
            }
        }

        private void OnLoad()
        {
            UpdateView();
        }

        private void OnUnload()
        {
            UnsubscribeEvents();
        }
    }
}
