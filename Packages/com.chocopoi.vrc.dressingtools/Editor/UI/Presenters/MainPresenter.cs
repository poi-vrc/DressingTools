using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.UIBase.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class MainPresenter : IMainPresenter
    {
        private IMainView mainView;

        public MainPresenter(IMainView mainView)
        {
            this.mainView = mainView;
        }

        public void StartDressingWizard()
        {
            // TODO: reset dressing tab?
            mainView.SwitchTab(1);
        }

        public void AddToCabinet(DTCabinet cabinet, DTWearableConfig config, GameObject wearableGameObject)
        {
            DTEditorUtils.AddCabinetWearable(cabinet, config, wearableGameObject);

            // TODO: reset dressing tab?
            // return to cabinet page
            mainView.SwitchTab(0);
        }
    }
}
