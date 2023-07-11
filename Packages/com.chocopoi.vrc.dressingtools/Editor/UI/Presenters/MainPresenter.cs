using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.UIBase.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using Newtonsoft.Json;

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

        public void AddToCabinet(DTCabinet cabinet, DTWearableConfig config)
        {
            var cabinetWearable = new DTCabinetWearable(config)
            {
                // empty references
                objectReferences = new DTGameObjectReference[0],
                // serialize a json copy for backward compatibility backup 
                serializedJson = JsonConvert.SerializeObject(config, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                })
            };

            cabinet.wearables.Add(cabinetWearable);

            // TODO: reset dressing tab?
            // return to cabinet page
            mainView.SwitchTab(0);
        }
    }
}
