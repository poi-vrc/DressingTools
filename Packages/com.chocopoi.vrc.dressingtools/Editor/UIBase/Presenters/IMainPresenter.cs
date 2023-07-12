using Chocopoi.DressingTools.Cabinet;
using UnityEngine;

namespace Chocopoi.DressingTools.UIBase.Presenters
{
    internal interface IMainPresenter
    {
        void StartDressingWizard();
        void AddToCabinet(DTCabinet cabinet, GameObject wearableGameObject, DTWearableConfig config);
    }
}
