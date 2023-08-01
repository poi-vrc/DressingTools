using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable;
using Newtonsoft.Json;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class CabinetPresenter
    {
        private ICabinetSubView _view;

        public CabinetPresenter(ICabinetSubView view)
        {
            _view = view;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.AddWearableButtonClick += OnAddWearableButtonClick;
            _view.CreateCabinetButtonClick += OnCreateCabinetButtonClick;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.AddWearableButtonClick -= OnAddWearableButtonClick;
            _view.CreateCabinetButtonClick -= OnCreateCabinetButtonClick;
        }

        private void UpdateView()
        {
            var cabinets = DTEditorUtils.GetAllCabinets();

            if (cabinets.Length == 0)
            {
                _view.ShowCabinetWearables = false;
                _view.ShowCreateCabinetWizard = true;
                return;
            }

            _view.ShowCabinetWearables = true;
            _view.ShowCreateCabinetWizard = false;

            // cabinet selection dropdown
            string[] cabinetOptions = new string[cabinets.Length];
            for (var i = 0; i < cabinets.Length; i++)
            {
                cabinetOptions[i] = cabinets[i].avatarGameObject != null ? cabinets[i].avatarGameObject.name : string.Format("Cabinet {0} (No GameObject Attached)", i + 1);
            }
            _view.AvailableCabinetSelections = cabinetOptions;

            if (_view.SelectedCabinetIndex < 0 || _view.SelectedCabinetIndex >= cabinets.Length)
            {
                // invalid selected cabinet index, setting it back to 0
                _view.SelectedCabinetIndex = 0;
            }

            // update selected cabinet view
            var cabinet = cabinets[_view.SelectedCabinetIndex];

            _view.CabinetAvatarGameObject = cabinet.avatarGameObject;
            _view.CabinetAvatarArmatureName = cabinet.avatarArmatureName;

            var wearables = cabinet.GetWearables();

            _view.WearablePreviews.Clear();
            foreach (var wearable in wearables)
            {
                var config = JsonConvert.DeserializeObject<DTWearableConfig>(wearable.configJson);

                _view.WearablePreviews.Add(new WearablePreview()
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
            _view.SelectTab(1);
        }

        private void OnCreateCabinetButtonClick()
        {
            DTEditorUtils.GetAvatarCabinet(_view.SelectedCreateCabinetGameObject, true);
            UpdateView();
        }
    }
}
