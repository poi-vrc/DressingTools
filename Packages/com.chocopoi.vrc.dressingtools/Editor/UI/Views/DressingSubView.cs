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
        private DressingPresenter presenter_;

        public event Action TargetAvatarOrWearableChange;
        public event Action AddToCabinetButtonClick;
        public GameObject TargetAvatar { get => targetAvatar_; set => targetAvatar_ = value; }
        public GameObject TargetWearable { get => targetWearable_; set => targetWearable_ = value; }
        public DTWearableConfig Config { get; set; }
        public bool ShowAvatarNoExistingCabinetHelpbox { get; set; }
        public bool DisableAllButtons { get; set; }
        public bool DisableAddToCabinetButton { get; set; }

        private IMainView mainView_;
        private GameObject targetAvatar_;
        private GameObject targetWearable_;
        private WearableConfigView configView_;

        public DressingSubView(IMainView mainView)
        {
            mainView_ = mainView;
            presenter_ = new DressingPresenter(this);

            targetAvatar_ = null;
            targetWearable_ = null;

            ShowAvatarNoExistingCabinetHelpbox = true;
            DisableAllButtons = true;
            DisableAddToCabinetButton = true;
            Config = new DTWearableConfig();

            configView_ = new WearableConfigView(this);
        }

        public void SelectTab(int selectedTab)
        {
            mainView_.SelectedTab = selectedTab;
        }

        public void ResetConfigView()
        {
            // reset parameters
            TargetAvatar = null;
            TargetWearable = null;
            Config = new DTWearableConfig();

            // force update the config view
            configView_.RaiseForceUpdateView();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            configView_.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            configView_.OnDisable();
        }

        public override void OnGUI()
        {
            GameObjectField("Avatar", ref targetAvatar_, true, TargetAvatarOrWearableChange);

            if (ShowAvatarNoExistingCabinetHelpbox)
            {
                HelpBox("The selected avatar has no existing cabinet.", MessageType.Error);
            }

            GameObjectField("Wearable", ref targetWearable_, true, TargetAvatarOrWearableChange);

            configView_.OnGUI();

            BeginHorizontal();
            {
                BeginDisabled(!configView_.IsValid());
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
