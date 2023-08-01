using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UI.Views;
using Chocopoi.DressingTools.UIBase;
using Chocopoi.DressingTools.UIBase.Views;

namespace Chocopoi.DressingTools.UI.View
{
    internal class MainView : EditorViewBase, IMainView
    {
        public int SelectedTab { get => _selectedTab; set => _selectedTab = value; }

        private MainPresenter _presenter;
        private ICabinetSubView _cabinetSubView;
        private IDressingSubView _dressingSubView;
        private ISettingsSubView _settingsSubView;
        private int _selectedTab;

        public MainView()
        {
            _presenter = new MainPresenter(this);
            _cabinetSubView = new CabinetSubView(this);
            _dressingSubView = new DressingSubView(this);
            _settingsSubView = new SettingsSubView(this);
        }

        public void ForceUpdateCabinetSubView()
        {
            _cabinetSubView.RaiseForceUpdateViewEvent();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            _cabinetSubView.OnEnable();
            _dressingSubView.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            _cabinetSubView.OnDisable();
            _dressingSubView.OnDisable();
        }

        public override void OnGUI()
        {
            DTLogo.Show();

            Toolbar(ref _selectedTab, new string[] { "Cabinet", "Dressing", "Settings" });

            if (_selectedTab == 0)
            {
                _cabinetSubView.OnGUI();
            }
            else if (_selectedTab == 1)
            {
                _dressingSubView.OnGUI();
            }
            else if (_selectedTab == 2)
            {
                _settingsSubView.OnGUI();
            }
        }
    }
}
