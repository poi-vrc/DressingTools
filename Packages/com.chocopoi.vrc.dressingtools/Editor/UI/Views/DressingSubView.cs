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
        public event Action AddToCabinetButtonClick;
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
        private WearableConfigView _configView;

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

            _configView = new WearableConfigView(this);
        }

        public void SelectTab(int selectedTab)
        {
            _mainView.SelectedTab = selectedTab;
        }

        public void ResetConfigView()
        {
            // reset parameters
            TargetAvatar = null;
            TargetWearable = null;
            Config = new DTWearableConfig();

            // force update the config view
            _configView.RaiseForceUpdateView();
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

            _configView.OnGUI();

            BeginHorizontal();
            {
                BeginDisabled(!_configView.IsValid());
                {
                    BeginDisabled(DisableAddToCabinetButton);
                    {
                        Button("Add to cabinet", AddToCabinetButtonClick);
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
