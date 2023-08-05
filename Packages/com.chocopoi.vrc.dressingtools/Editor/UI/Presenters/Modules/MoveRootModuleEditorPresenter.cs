using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters.Modules
{
    internal class MoveRootModuleEditorPresenter
    {
        private IMoveRootModuleEditorView _view;
        private IModuleEditorViewParent _parentView;
        private MoveRootModule _module;

        public MoveRootModuleEditorPresenter(IMoveRootModuleEditorView view, IModuleEditorViewParent parentView, MoveRootModule module)
        {
            _view = view;
            _parentView = parentView;
            _module = module;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.TargetAvatarOrWearableChange += OnTargetAvatarOrWearableChange;
            _view.MoveToGameObjectFieldChange += OnMoveToGameObjectFieldChange;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.TargetAvatarOrWearableChange -= OnTargetAvatarOrWearableChange;
            _view.MoveToGameObjectFieldChange -= OnMoveToGameObjectFieldChange;
        }

        private void OnTargetAvatarOrWearableChange()
        {
            ApplyMoveToGameObjectFieldChanges();
            UpdateView();
        }

        private void UpdateView()
        {
            if (_parentView.TargetAvatar != null)
            {
                _view.ShowSelectAvatarFirstHelpBox = false;
                _view.MoveToGameObject = _module.avatarPath != null ? _parentView.TargetAvatar.transform.Find(_module.avatarPath)?.gameObject : null;
            }
            else
            {
                _view.ShowSelectAvatarFirstHelpBox = true;
            }
        }

        private void OnMoveToGameObjectFieldChange()
        {
            ApplyMoveToGameObjectFieldChanges();
        }

        private void ApplyMoveToGameObjectFieldChanges()
        {
            if (_parentView.TargetAvatar != null && _view.MoveToGameObject != null && DTRuntimeUtils.IsGrandParent(_parentView.TargetAvatar.transform, _view.MoveToGameObject.transform))
            {
                _view.IsGameObjectInvalid = false;

                // renew path if valid
                _module.avatarPath = DTRuntimeUtils.GetRelativePath(_view.MoveToGameObject.transform, _parentView.TargetAvatar.transform);
            }
            else
            {
                // show helpbox that the path is invalid
                _view.IsGameObjectInvalid = true;
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
