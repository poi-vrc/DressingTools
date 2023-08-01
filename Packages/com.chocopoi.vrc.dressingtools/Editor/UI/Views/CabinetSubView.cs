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
        public int SelectedCabinetIndex { get => _selectedCabinetIndex; set => _selectedCabinetIndex = value; }
        public string[] AvailableCabinetSelections { get; set; }
        public GameObject CabinetAvatarGameObject { get => _cabinetAvatarGameObject; set => _cabinetAvatarGameObject = value; }
        public string CabinetAvatarArmatureName { get => _cabinetAvatarArmatureName; set => _cabinetAvatarArmatureName = value; }
        public List<WearablePreview> WearablePreviews { get; set; }
        public GameObject SelectedCreateCabinetGameObject { get => _selectedCreateCabinetGameObject; }

        private IMainView _mainView;
        private CabinetPresenter _cabinetPresenter;
        private GameObject _selectedCreateCabinetGameObject;
        private int _selectedCabinetIndex;
        private GameObject _cabinetAvatarGameObject;
        private string _cabinetAvatarArmatureName;

        public CabinetSubView(IMainView mainView)
        {
            _mainView = mainView;
            _cabinetPresenter = new CabinetPresenter(this);
            _selectedCabinetIndex = 0;

            ShowCreateCabinetWizard = false;
            ShowCabinetWearables = false;
            AvailableCabinetSelections = new string[0];
            WearablePreviews = new List<WearablePreview>();
        }

        public void SelectTab(int selectedTab)
        {
            _mainView.SelectedTab = selectedTab;
        }

        public override void OnGUI()
        {
            if (ShowCreateCabinetWizard)
            {
                Label("There are no existing cabinets. Create one below for your avatar:");
                GameObjectField("Avatar", ref _selectedCreateCabinetGameObject, true);
                Button("Create cabinet", CreateCabinetButtonClick);
            }

            if (ShowCabinetWearables)
            {
                // create dropdown menu for cabinet selection
                Popup("Cabinet", ref _selectedCabinetIndex, AvailableCabinetSelections);

                GameObjectField("Avatar", ref _cabinetAvatarGameObject, true);
                TextField("Armature Name", ref _cabinetAvatarArmatureName);

                var copy = new List<WearablePreview>(WearablePreviews);
                foreach (var preview in copy)
                {
                    Label(preview.name);
                    Button("Remove", preview.RemoveButtonClick);
                }

                Button("Add Wearable", AddWearableButtonClick);
            }
        }
    }
}
