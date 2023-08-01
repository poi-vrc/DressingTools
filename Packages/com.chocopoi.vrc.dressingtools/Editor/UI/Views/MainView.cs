using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UI.Views;
using Chocopoi.DressingTools.UIBase;
using Chocopoi.DressingTools.UIBase.Views;

namespace Chocopoi.DressingTools.UI.View
{
    internal class MainView : EditorViewBase, IMainView
    {
        public int SelectedTab { get => selectedTab_; set => selectedTab_ = value; }

        private MainPresenter presenter_;
        private ICabinetSubView cabinetSubView_;
        private IDressingSubView dressingSubView_;
        private ISettingsSubView settingsSubView_;
        private int selectedTab_;

        public MainView()
        {
            presenter_ = new MainPresenter(this);
            cabinetSubView_ = new CabinetSubView(this);
            dressingSubView_ = new DressingSubView(this);
            settingsSubView_ = new SettingsSubView(this);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            cabinetSubView_.OnEnable();
            dressingSubView_.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            cabinetSubView_.OnDisable();
            dressingSubView_.OnDisable();
        }

        public override void OnGUI()
        {
            DTLogo.Show();

            Toolbar(ref selectedTab_, new string[] { "Cabinet", "Dressing", "Settings" });

            if (selectedTab_ == 0)
            {
                cabinetSubView_.OnGUI();
            }
            else if (selectedTab_ == 1)
            {
                dressingSubView_.OnGUI();
            }
            else if (selectedTab_ == 2)
            {
                settingsSubView_.OnGUI();
            }
        }
    }
}
