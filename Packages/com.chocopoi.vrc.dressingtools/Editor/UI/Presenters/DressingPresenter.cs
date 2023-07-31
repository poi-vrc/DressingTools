using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class DressingPresenter
    {
        private IDressingSubView view_;

        public DressingPresenter(IDressingSubView view)
        {
            view_ = view;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            view_.Load += OnLoad;
            view_.Unload += OnUnload;

            view_.TargetAvatarOrWearableChange += OnTargetAvatarOrWearableChange;
            view_.AddToCabinetButtonClick += OnAddToCabinetButtonClick;
        }

        private void UnsubscribeEvents()
        {
            view_.Load -= OnLoad;
            view_.Unload -= OnUnload;

            view_.TargetAvatarOrWearableChange -= OnTargetAvatarOrWearableChange;
            view_.AddToCabinetButtonClick -= OnAddToCabinetButtonClick;
        }

        private void UpdateView()
        {
            var cabinet = DTEditorUtils.GetAvatarCabinet(view_.TargetAvatar);
            var cabinetIsNull = cabinet == null;
            view_.ShowAvatarNoExistingCabinetHelpbox = cabinetIsNull;
            view_.DisableAddToCabinetButton = cabinetIsNull;
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
            var cabinet = DTEditorUtils.GetAvatarCabinet(view_.TargetAvatar);

            if (cabinet == null)
            {
                return;
            }

            DTEditorUtils.AddCabinetWearable(cabinet, view_.Config, view_.TargetWearable);

            // reset
            view_.TargetAvatar = null;
            view_.TargetWearable = null;
            view_.Config = new DTWearableConfig();
            view_.ForceUpdateConfigView();
        }
    }
}
