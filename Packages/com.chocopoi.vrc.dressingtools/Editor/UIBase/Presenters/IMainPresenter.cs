using Chocopoi.DressingTools.Cabinet;

namespace Chocopoi.DressingTools.UIBase.Presenters
{
    internal interface IMainPresenter
    {
        void StartDressingWizard();
        void AddToCabinet(DTCabinet cabinet, DTWearableConfig config);
    }
}
