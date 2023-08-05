using System;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views
{
    internal class DressingSubView : EditorViewBase, IDressingSubView, IWearableConfigViewParent
    {
        public event Action TargetAvatarOrWearableChange;
        public event Action DoAddToCabinetEvent;
        public GameObject TargetAvatar { get => _targetAvatar; set => _targetAvatar = value; }
        public GameObject TargetWearable { get => _targetWearable; set => _targetWearable = value; }
        public DTWearableConfig Config { get; set; }
        public bool ShowAvatarNoExistingCabinetHelpbox { get; set; }
        public bool DisableAllButtons { get; set; }
        public bool DisableAddToCabinetButton { get; set; }

        private DressingPresenter _presenter;
        private IMainView _mainView;
        private GameObject _targetAvatar;
        private GameObject _targetWearable;
        private WearableSetupWizardView _wizardView;
        private WearableConfigView _configView;
        private int _currentMode;

        public DressingSubView(IMainView mainView)
        {
            _mainView = mainView;
            _presenter = new DressingPresenter(this);

            _targetAvatar = null;
            _targetWearable = null;

            ShowAvatarNoExistingCabinetHelpbox = true;
            DisableAllButtons = true;
            DisableAddToCabinetButton = true;
            Config = new DTWearableConfig();

            _wizardView = new WearableSetupWizardView(this);
            _configView = new WearableConfigView(this);
        }

        public bool IsConfigValid()
        {
            return _currentMode == 0 ? _wizardView.IsValid() : _configView.IsValid();
        }

        public void SelectTab(int selectedTab)
        {
            _mainView.SelectedTab = selectedTab;
        }

        public void ForceUpdateCabinetSubView()
        {
            _mainView.ForceUpdateCabinetSubView();
        }

        public void RaiseDoAddToCabinetEvent()
        {
            DoAddToCabinetEvent?.Invoke();
        }

        public void ResetWizardAndConfigView()
        {
            // reset parameters
            TargetAvatar = null;
            TargetWearable = null;
            Config = new DTWearableConfig();

            _wizardView.CurrentStep = 0;
            _wizardView.TargetAvatar = null;
            _wizardView.TargetWearable = null;
            _wizardView.RaiseForceUpdateViewEvent();

            // force update the config view
            _configView.RaiseForceUpdateViewEvent();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            _configView.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            _configView.OnDisable();
        }

        public override void OnGUI()
        {
            GameObjectField("Avatar", ref _targetAvatar, true, TargetAvatarOrWearableChange);

            if (ShowAvatarNoExistingCabinetHelpbox)
            {
                HelpBox("The selected avatar has no existing cabinet.", MessageType.Error);
            }

            GameObjectField("Wearable", ref _targetWearable, true, TargetAvatarOrWearableChange);

            HorizontalLine();

            BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                // TODO: ask wizard to write back data to here on mode change
                Toolbar(ref _currentMode, new string[] { "Wizard", "Advanced" });
            }
            EndHorizontal();

            Separator();

            if (_currentMode == 0)
            {
                // Wizard mode
                _wizardView.OnGUI();
            }
            else if (_currentMode == 1)
            {
                // Fully-custom advanced mode
                _configView.OnGUI();

                BeginHorizontal();
                {
                    BeginDisabled(!_configView.IsValid());
                    {
                        BeginDisabled(DisableAddToCabinetButton);
                        {
                            Button("Add to cabinet", DoAddToCabinetEvent);
                        }
                        EndDisabled();

                        Button("Save to file");
                    }
                    EndDisabled();
                }
                EndHorizontal();
            }
        }
    }
}
