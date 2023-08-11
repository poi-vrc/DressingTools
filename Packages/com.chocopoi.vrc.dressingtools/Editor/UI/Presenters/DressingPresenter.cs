using System;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class DressingPresenter
    {
        private IDressingSubView _view;

        public DressingPresenter(IDressingSubView view)
        {
            _view = view;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.ForceUpdateView += OnForceUpdateView;
            _view.TargetAvatarOrWearableChange += OnTargetAvatarOrWearableChange;
            _view.DoAddToCabinetEvent += OnAddToCabinetButtonClick;
            _view.DressingModeChange += OnDressingModeChange;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.ForceUpdateView -= OnForceUpdateView;
            _view.TargetAvatarOrWearableChange -= OnTargetAvatarOrWearableChange;
            _view.DoAddToCabinetEvent -= OnAddToCabinetButtonClick;
            _view.DressingModeChange -= OnDressingModeChange;
        }

        private void OnDressingModeChange()
        {
            // ask if really switch back to wizard mode
            if (_view.SelectedDressingMode == 0 && !EditorUtility.DisplayDialog("DressingTools", "Switching back to wizard mode will do auto-setup and wipe your existing configuration here.\nAre you sure?", "Yes", "No"))
            {
                _view.SelectedDressingMode = 1;
                return;
            }

            // generate config
            _view.WizardGenerateConfig();
        }

        private void OnForceUpdateView()
        {
            UpdateView();
        }

        private void UpdateView()
        {
            var cabinet = DTEditorUtils.GetAvatarCabinet(_view.TargetAvatar);
            var cabinetIsNull = cabinet == null;
            _view.ShowAvatarNoExistingCabinetHelpbox = cabinetIsNull;
            _view.DisableAddToCabinetButton = cabinetIsNull;
        }

        private void OnLoad()
        {
            UpdateView();
        }

        private void OnUnload()
        {
            UnsubscribeEvents();
        }

        private void OnTargetAvatarOrWearableChange()
        {
            UpdateView();
        }

        private void OnAddToCabinetButtonClick()
        {
            Debug.Log("Add");
            if (!_view.IsConfigValid())
            {
                Debug.Log("[DressingTools] Invalid configuration. Cannot proceed adding to cabinet");
                return;
            }

            var cabinet = DTEditorUtils.GetAvatarCabinet(_view.TargetAvatar);

            if (cabinet == null)
            {
                return;
            }

            DTEditorUtils.AddCabinetWearable(cabinet, _view.Config, _view.TargetWearable);

            // reset and return
            _view.ResetWizardAndConfigView();
            _view.SelectTab(0);

            _view.ForceUpdateCabinetSubView();
        }
    }
}
