using System;
using System.Collections.Generic;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views
{
    internal class CabinetSubView : EditorViewBase, ICabinetSubView
    {
        public event Action CreateCabinetButtonClick;
        public event Action AddWearableButtonClick;

        public bool ShowCreateCabinetWizard { get; set; }
        public bool ShowCabinetWearables { get; set; }
        public int SelectedCabinetIndex { get => selectedCabinetIndex_; set => selectedCabinetIndex_ = value; }
        public string[] AvailableCabinetSelections { get; set; }
        public GameObject CabinetAvatarGameObject { get => cabinetAvatarGameObject_; set => cabinetAvatarGameObject_ = value; }
        public string CabinetAvatarArmatureName { get => cabinetAvatarArmatureName_; set => cabinetAvatarArmatureName_ = value; }
        public List<WearablePreview> WearablePreviews { get; set; }
        public GameObject SelectedCreateCabinetGameObject { get => selectedCreateCabinetGameObject_; }

        private CabinetPresenter cabinetPresenter_;
        private GameObject selectedCreateCabinetGameObject_;
        private int selectedCabinetIndex_;
        private GameObject cabinetAvatarGameObject_;
        private string cabinetAvatarArmatureName_;

        public CabinetSubView(IMainView mainView)
        {
            cabinetPresenter_ = new CabinetPresenter(this);
            selectedCabinetIndex_ = 0;

            ShowCreateCabinetWizard = false;
            ShowCabinetWearables = false;
            AvailableCabinetSelections = new string[0];
            WearablePreviews = new List<WearablePreview>();
        }

        public override void OnGUI()
        {
            if (ShowCreateCabinetWizard)
            {
                Label("There are no existing cabinets. Create one below for your avatar:");
                GameObjectField("Avatar", ref selectedCreateCabinetGameObject_, true);
                Button("Create cabinet", CreateCabinetButtonClick);
            }

            if (ShowCabinetWearables)
            {
                // create dropdown menu for cabinet selection
                Popup("Cabinet", ref selectedCabinetIndex_, AvailableCabinetSelections);

                GameObjectField("Avatar", ref cabinetAvatarGameObject_, true);
                TextField("Armature Name", ref cabinetAvatarArmatureName_);

                foreach (var preview in WearablePreviews)
                {
                    Label(preview.name);
                    Button("Remove", preview.RemoveButtonClick);
                }

                Button("Add Wearable", AddWearableButtonClick);
            }
        }
    }
}
