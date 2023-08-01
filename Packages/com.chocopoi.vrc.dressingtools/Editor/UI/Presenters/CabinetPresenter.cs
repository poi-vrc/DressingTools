using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable;
using Newtonsoft.Json;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class CabinetPresenter
    {
        private ICabinetSubView view_;

        public CabinetPresenter(ICabinetSubView view)
        {
            view_ = view;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            view_.Load += OnLoad;
            view_.Unload += OnUnload;

            view_.AddWearableButtonClick += OnAddWearableButtonClick;
            view_.CreateCabinetButtonClick += OnCreateCabinetButtonClick;
        }

        private void UnsubscribeEvents()
        {
            view_.Load -= OnLoad;
            view_.Unload -= OnUnload;

            view_.AddWearableButtonClick -= OnAddWearableButtonClick;
            view_.CreateCabinetButtonClick -= OnCreateCabinetButtonClick;
        }

        private void UpdateView()
        {
            var cabinets = DTEditorUtils.GetAllCabinets();

            if (cabinets.Length == 0)
            {
                view_.ShowCabinetWearables = false;
                view_.ShowCreateCabinetWizard = true;
                return;
            }

            view_.ShowCabinetWearables = true;
            view_.ShowCreateCabinetWizard = false;

            // cabinet selection dropdown
            string[] cabinetOptions = new string[cabinets.Length];
            for (var i = 0; i < cabinets.Length; i++)
            {
                cabinetOptions[i] = cabinets[i].avatarGameObject != null ? cabinets[i].avatarGameObject.name : string.Format("Cabinet {0} (No GameObject Attached)", i + 1);
            }
            view_.AvailableCabinetSelections = cabinetOptions;

            if (view_.SelectedCabinetIndex < 0 || view_.SelectedCabinetIndex >= cabinets.Length)
            {
                // invalid selected cabinet index, setting it back to 0
                view_.SelectedCabinetIndex = 0;
            }

            // update selected cabinet view
            var cabinet = cabinets[view_.SelectedCabinetIndex];

            view_.CabinetAvatarGameObject = cabinet.avatarGameObject;
            view_.CabinetAvatarArmatureName = cabinet.avatarArmatureName;

            var wearables = cabinet.GetWearables();

            view_.WearablePreviews.Clear();
            foreach (var wearable in wearables)
            {
                var config = JsonConvert.DeserializeObject<DTWearableConfig>(wearable.configJson);

                view_.WearablePreviews.Add(new WearablePreview()
                {
                    name = config != null ? config.info.name : "(Unable to load configuration)",
                    RemoveButtonClick = () =>
                    {
                        DTEditorUtils.RemoveCabinetWearable(cabinet, wearable);
                        UpdateView();
                    }
                });
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

        private void OnAddWearableButtonClick()
        {
            // TODO: start dressing wizard
            view_.SelectTab(1);
        }

        private void OnCreateCabinetButtonClick()
        {
            DTEditorUtils.GetAvatarCabinet(view_.SelectedCreateCabinetGameObject, true);
            UpdateView();
        }
    }
}
