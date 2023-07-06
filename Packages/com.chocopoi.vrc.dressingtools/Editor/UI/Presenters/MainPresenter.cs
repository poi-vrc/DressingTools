using Chocopoi.DressingTools.UIBase.Presenters;
using Chocopoi.DressingTools.UIBase.Views;

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
    }
}
