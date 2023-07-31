using Chocopoi.DressingTools.UIBase.Views;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class MainPresenter
    {
        private IMainView view_;

        public MainPresenter(IMainView view)
        {
            view_ = view;
        }
    }
}
