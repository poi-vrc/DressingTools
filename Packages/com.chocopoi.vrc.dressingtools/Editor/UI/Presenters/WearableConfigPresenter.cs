using Chocopoi.DressingTools.UIBase.Presenters;
using Chocopoi.DressingTools.UIBase.Views;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class WearableConfigPresenter : IWearableConfigPresenter
    {
        private IWearableConfigView wearableConfigView;

        public WearableConfigPresenter(IWearableConfigView wearableConfigView)
        {
            this.wearableConfigView = wearableConfigView;
        }
    }
}
