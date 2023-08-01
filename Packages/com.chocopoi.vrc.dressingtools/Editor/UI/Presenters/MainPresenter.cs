using Chocopoi.DressingTools.UIBase.Views;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class MainPresenter
    {
        private IMainView _view;

        public MainPresenter(IMainView view)
        {
            _view = view;
        }
    }
}
